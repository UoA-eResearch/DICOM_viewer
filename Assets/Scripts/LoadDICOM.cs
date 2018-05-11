using UnityEngine;
using Dicom.Imaging;
using System.IO;
using Dicom.Media;
using Dicom;
using System.Linq;
using Dicom.Imaging.LUT;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.XR.WSA.Input;
using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Examples.InteractiveElements;
using System;
using HoloToolkit.Unity.UX;
using UnityEngine.SceneManagement;

public class LoadDICOM : MonoBehaviour
{

	public GameObject quadPrefab;
	public GameObject annotationPrefab;
	public GameObject testQuad;
	public TextMesh status;
	private Dictionary<GameObject, DicomDirectoryRecord> directoryMap;
	private Dictionary<DicomDirectoryRecord, string> rootDirectoryMap;
	private GestureRecognizer recognizer;
	private Vector3 offset = Vector3.zero;
	private Dictionary<GameObject, bool> openedItems;
	private GameObject selectedObject = null;

	string GetDicomTag(DicomDirectoryRecord record, DicomTag tag)
	{
		return string.Join(",", record.Get<string[]>(tag, new string[] { "" }));
	}

	void PrintTagsForRecord(DicomDirectoryRecord record)
	{
#if UNITY_EDITOR
		foreach (var field in typeof(DicomTag).GetFields())
		{
			try
			{
				Debug.Log(field.Name + ":" + string.Join(",", record.Get<string[]>((DicomTag)field.GetValue(null))));
			}
			catch (System.Exception)
			{
			}
		}
#endif
	}

	static short[] ConvertByteArray(byte[] bytes)
	{
		var size = bytes.Length / sizeof(short);
		var shorts = new short[size];
		for (var index = 0; index < size; index++)
		{
			shorts[index] = System.BitConverter.ToInt16(bytes, index * sizeof(short));
		}
		return shorts;
	}

	static Texture2D DicomToTex2D(DicomImage image)
	{
		var pixels = image.PixelData;
		var bytes = pixels.GetFrame(0).Data;
		var shorts = ConvertByteArray(bytes);
		var rescaleIntercept = image.Dataset.Get<float>(DicomTag.RescaleIntercept, -1024f);
		var rescaleSlope = image.Dataset.Get<float>(DicomTag.RescaleSlope, 1f);
		/*
		Debug.Log(pixels.Height + "," + pixels.Width);
		Debug.Log(shorts.Length);
		Debug.Log(shorts.Min() + "-" + shorts.Average(x => (int)x) + "-" + shorts.Max());
		Debug.Log(image.WindowCenter + "," + image.WindowWidth + "," + image.Scale);
		Debug.Log(rescale + "," + rescaleSlope);
		*/
		var tex = new Texture2D(pixels.Width, pixels.Height);
		var colors = new UnityEngine.Color32[shorts.Length];
		for (int i = 0; i < shorts.Length; i++)
		{
			double intensity = shorts[i];
			intensity = intensity * rescaleSlope + rescaleIntercept;
			// Threshold based on WindowCenter and WindowWidth
			intensity -= image.WindowCenter - image.WindowWidth;
			// Remap to 0-255 range and clamp
			intensity = intensity / (image.WindowCenter + image.WindowWidth) * 255;
			intensity = Mathf.Clamp((int)intensity, 0, 255);
			var color = image.GrayscaleColorMap[(int)intensity];
			//Debug.Log("intensity at " + x + "," + y + "=" + intensity);
			colors[i] = new UnityEngine.Color32(color.R, color.G, color.B, color.A);
		}
		tex.SetPixels32(colors);
		tex.Apply();

		return tex;
	}

	static DicomDirectoryRecord GetSeries(DicomDirectoryRecord record)
	{
		if (record.DirectoryRecordType == "IMAGE")
		{
			return record;
		}
		while (record.DirectoryRecordType != "SERIES")
		{
			record = record.LowerLevelDirectoryRecord;
		}
		return record;
	}

	public Texture2D GetImageForRecord(DicomDirectoryRecord record, int frame = -1)
	{
		var directory = rootDirectoryMap[record];
		var series = GetSeries(record);
		var tex = new Texture2D(1, 1);
		var absoluteFilename = "";
		var instanceNumbers = series.LowerLevelDirectoryRecordCollection.Select(x => x.Get<int>(DicomTag.InstanceNumber)).OrderBy(x => x).ToArray();
		if (frame == -1) // get thumbnail - get midpoint image
		{
			frame = instanceNumbers[instanceNumbers.Length / 2];
		}
		try
		{
			var imageRecord = series.LowerLevelDirectoryRecordCollection.First(x => x.Get<int>(DicomTag.InstanceNumber) == frame);
			var filename = Path.Combine(imageRecord.Get<string[]>(DicomTag.ReferencedFileID));
			absoluteFilename = Path.Combine(directory, filename);
			Debug.Log("load image " + absoluteFilename);
			var img = new DicomImage(absoluteFilename);
			tex = DicomToTex2D(img);
		}
		catch (InvalidOperationException)
		{
			Debug.LogError("series does not contain an image of InstanceNumber=" + frame + "- valid instance numbers = " + string.Join(",", instanceNumbers));
		}
		catch (System.Exception)
		{
			Debug.LogError("Failed to load " + absoluteFilename);
		}
		return tex;
	}

	// Use this for initialization
	void Start()
	{
		Debug.Log("Loading DICOM DICT");
		var dict = new DicomDictionary();
		dict.Load(Application.dataPath + "/StreamingAssets/Dictionaries/DICOM Dictionary.xml", DicomDictionaryFormat.XML);
		DicomDictionary.Default = dict;
#if !UNITY_EDITOR && UNITY_METRO
		var root = Application.persistentDataPath;
#else
		var root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
#endif
		var zip = Path.Combine(root, "DICOM.zip");
		var path = Path.Combine(root, "DICOM");
		if (!File.Exists(zip) && !Directory.Exists(path))
		{
			status.text = "ERROR: No zip file found!";
			return;
		}
		if (File.Exists(zip) && !Directory.Exists(path))
		{
			Debug.Log("unzipping..");
			status.text = "unzipping...";
			System.IO.Compression.ZipFile.ExtractToDirectory(zip, path);
			Debug.Log("unzip done!");
		}
		var offset = 0;
		directoryMap = new Dictionary<GameObject, DicomDirectoryRecord>();
		rootDirectoryMap = new Dictionary<DicomDirectoryRecord, string>();
		openedItems = new Dictionary<GameObject, bool>();
		var directories = Directory.GetDirectories(path);
		if (directories.Length == 0)
		{
			status.text = "ERROR: No directories found!";
			return;
		}
		status.text = "Loading...";
		foreach (var directory in Directory.GetDirectories(path))
		{
			var directoryName = Path.GetFileName(directory);
			Debug.Log("--DIRECTORY--" + directoryName);
			var dd = DicomDirectory.Open(Path.Combine(directory, "DICOMDIR"));
			rootDirectoryMap[dd.RootDirectoryRecord] = directory;
			var tex = GetImageForRecord(dd.RootDirectoryRecord);
			var quad = Instantiate(quadPrefab, transform);
			quad.GetComponent<Renderer>().material.mainTexture = tex;
			quad.transform.localPosition += new Vector3(offset, 0, 0);
			quad.transform.Find("Canvas").Find("title").GetComponent<Text>().text = "Directory: " + directoryName;
			quad.name = directory;
			directoryMap[quad] = dd.RootDirectoryRecord;
			openedItems[quad] = false;
			quad.tag = "directory";
			offset += 1;
		}
		recognizer = new GestureRecognizer();
		recognizer.TappedEvent += Recognizer_TappedEvent;
		recognizer.StartCapturingGestures();
		status.text = "";

#if UNITY_EDITOR
		testQuad.SetActive(true);
		var firstStudy = directoryMap.First().Value.LowerLevelDirectoryRecord;
		// worst case series - most images
		int largest = 0;
		DicomDirectoryRecord largestSeries = firstStudy.LowerLevelDirectoryRecord;
		foreach (var series in firstStudy.LowerLevelDirectoryRecordCollection)
		{
			var n_images = series.LowerLevelDirectoryRecordCollection.Count();
			if (n_images > largest)
			{
				largest = n_images;
				largestSeries = series;
			}
		}
		var seriesHandler = testQuad.GetComponent<OpenSeriesHandler>();
		seriesHandler.record = largestSeries;

		var modality = GetDicomTag(largestSeries, DicomTag.Modality);
		var seriesDesc = GetDicomTag(largestSeries, DicomTag.SeriesDescription);
		testQuad.name = "Series: " + modality + "\n" + seriesDesc;
		directoryMap[testQuad] = largestSeries;
		rootDirectoryMap[largestSeries] = rootDirectoryMap[directoryMap.First().Value];

		testQuad.GetComponent<TwoHandManipulatable>().enabled = true;
		testQuad.transform.Find("AppBar").gameObject.SetActive(true);
		Debug.Log(testQuad.transform.Find("AppBar").name);
		var slider = testQuad.transform.Find("Slider");
		slider.gameObject.SetActive(true);
		var sliderComponent = slider.GetComponent<SliderGestureControl>();
		sliderComponent.SetSpan(0, largest);
		sliderComponent.SetSliderValue(largest / 2f);
		testQuad.GetComponent<Renderer>().material.mainTexture = GetImageForRecord(largestSeries);
#endif
	}

	private void Recognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
	{
		RaycastHit hit;
		if (Physics.Raycast(headRay, out hit))
		{
			ClickObject(hit.collider.gameObject);
		}
	}

	void Close(GameObject go)
	{
		foreach (Transform child in go.transform)
		{
			if (child.name != "Canvas")
			{
				Destroy(child.gameObject);
			}
		}
		openedItems[go] = false;
	}

	void ClickObject(GameObject go)
	{
		if (go.tag == "opened_series")
		{
			selectedObject = go;
			var annotation = Instantiate(annotationPrefab, go.transform);
			annotation.transform.position = go.transform.position;
			return;
		}
		if (go == selectedObject) // object is already selected
		{
			return;
		}
		else
		{
			selectedObject = null;
		}
		if (!directoryMap.ContainsKey(go))
		{
			return;
		}
		if (openedItems.ContainsKey(go) && openedItems[go]) // clicking on an open directory or study - close it
		{
			Close(go);
			return;
		}
		foreach (var otherGo in GameObject.FindGameObjectsWithTag(go.tag)) // opening a directory or study - close all other directories or studies
		{
			if (openedItems[otherGo])
			{
				Close(otherGo);
			}
		}
		openedItems[go] = true; // this is now open
		var record = directoryMap[go];
		if (record.DirectoryRecordType == "SERIES") // opening a series - bring it out of the tree
		{
			var clone = Instantiate(go);
			clone.transform.localScale = go.transform.lossyScale;
			clone.transform.position = go.transform.position;
			clone.transform.Translate(0, 0, -.5f, Space.Self);
			selectedObject = clone;
			directoryMap[clone] = record;
			clone.tag = "opened_series";
			clone.GetComponent<TwoHandManipulatable>().enabled = true;
			clone.transform.Find("AppBar").gameObject.SetActive(true);
			var slider = clone.transform.Find("Slider");
			slider.gameObject.SetActive(true);
			var sliderComponent = slider.GetComponent<SliderGestureControl>();
			var n_images = record.LowerLevelDirectoryRecordCollection.Count();
			sliderComponent.SetSpan(0, n_images);
			sliderComponent.SetSliderValue(n_images / 2f);
			var openSeriesHandler = clone.GetComponent<OpenSeriesHandler>();
			openSeriesHandler.record = record;
			openSeriesHandler.loadDicomInstance = this;
			return;
		}
		var rootDirectory = rootDirectoryMap[record];
		var offset = 0;
		status.text = "Loading...";
		foreach (var subRecord in record.LowerLevelDirectoryRecordCollection)
		{
			rootDirectoryMap[subRecord] = rootDirectory;
			var desc = "";
			var quad = Instantiate(quadPrefab, go.transform);
			if (subRecord.DirectoryRecordType == "STUDY")
			{
				var studyDate = GetDicomTag(subRecord, DicomTag.StudyDate);
				var studyDesc = GetDicomTag(subRecord, DicomTag.StudyDescription);
				var studyComments = GetDicomTag(subRecord, DicomTag.StudyCommentsRETIRED);
				desc = "Study: " + studyDate + "\n" + studyDesc + "\n" + studyComments;
				quad.tag = "study";
			}
			else if (subRecord.DirectoryRecordType == "SERIES")
			{
				var modality = GetDicomTag(subRecord, DicomTag.Modality);
				var seriesDesc = GetDicomTag(subRecord, DicomTag.SeriesDescription);
				desc = "Series: " + modality + "\n" + seriesDesc;
				quad.tag = "series";
			}
			else if (subRecord.DirectoryRecordType == "IMAGE")
			{
				desc = "Image: " + subRecord.Get<string>(DicomTag.InstanceNumber);
				quad.tag = "image";
			}
			var tex = GetImageForRecord(subRecord);
			quad.GetComponent<Renderer>().material.mainTexture = tex;
			quad.transform.localPosition += new Vector3(offset, -2, 0);
			quad.transform.Find("Canvas").Find("title").GetComponent<Text>().text = desc;
			quad.name = desc.Replace("\n", ":");
			directoryMap[quad] = subRecord;
			openedItems[quad] = false;
			offset += 1;
		}
		status.text = "";
	}

	// Update is called once per frame
	void Update()
	{
		var t = transform;
		if (selectedObject)
		{
			t = selectedObject.transform;
		}
		if (Input.GetMouseButtonDown(0))
		{
			float distance_to_screen = Camera.main.WorldToScreenPoint(t.position).z;
			offset = t.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
		}
		if (Input.GetMouseButton(0))
		{
			float distance_to_screen = Camera.main.WorldToScreenPoint(t.position).z;
			t.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen)) + offset;
		}
		if (Input.GetMouseButtonUp(0))
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				ClickObject(hit.collider.gameObject);
			}
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (!focus)
		{
#if !UNITY_EDITOR
			SceneManager.LoadScene("PIN");
#endif
		}
	}
}
