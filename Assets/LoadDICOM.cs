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

public class LoadDICOM : MonoBehaviour {

	public GameObject quadPrefab;
	private Dictionary<GameObject, DicomDirectoryRecord> directoryMap;
	private Dictionary<DicomDirectoryRecord, string> rootDirectoryMap;
	private GestureRecognizer recognizer;
	private Vector3 offset;

	void PrintTagsForRecord(DicomDirectoryRecord record)
	{
		#if UNITY_EDITOR
		foreach (var field in typeof(DicomTag).GetFields())
		{
			try
			{
				Debug.Log(field.Name + ":" + string.Join(",", record.Get<string[]>((DicomTag) field.GetValue(null))));
			}
			catch (System.Exception)
			{
			}
		}
		#endif
	}

	short[] ConvertByteArray(byte[] bytes)
	{
		var size = bytes.Length / sizeof(short);
		var shorts = new short[size];
		for (var index = 0; index < size; index++)
		{
			shorts[index] = System.BitConverter.ToInt16(bytes, index * sizeof(short));
		}
		return shorts;
	}

	Texture2D DicomToTex2D(DicomImage image)
	{
		var pixels = image.PixelData;
		var bytes = pixels.GetFrame(0).Data;
		var shorts = ConvertByteArray(bytes);
		var rescale = image.Dataset.Get<float>(DicomTag.RescaleIntercept, -1024f);
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
			intensity += rescale;
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

	DicomDirectoryRecord GetRepresentativeImageRecord(DicomDirectoryRecord record)
	{
		if (record.DirectoryRecordType == "IMAGE")
		{
			return record;
		}
		while (record.DirectoryRecordType != "SERIES")
		{
			record = record.LowerLevelDirectoryRecord;
		}
		return record.LowerLevelDirectoryRecordCollection.OrderBy(x => x.Get<string>(DicomTag.ReferencedFileID, 3)).First();
	}

	Texture2D GetImageForRecord(DicomDirectoryRecord record)
	{
		var directory = rootDirectoryMap[record];
		record = GetRepresentativeImageRecord(record);
		var filename = Path.Combine(record.Get<string[]>(DicomTag.ReferencedFileID));
		var absoluteFilename = Path.Combine(directory, filename);
		Debug.Log("load image " + absoluteFilename);
		var img = new DicomImage(absoluteFilename);
		var tex = DicomToTex2D(img);
		return tex;
	}

	// Use this for initialization
	void Start () {
		Debug.Log("Loading DICOM DICT");
		var dict = new DicomDictionary();
		dict.Load(Application.dataPath + "/StreamingAssets/Dictionaries/DICOM Dictionary.xml", DicomDictionaryFormat.XML);
		DicomDictionary.Default = dict;
#if !UNITY_EDITOR && UNITY_METRO
		var zipLocation = Windows.Storage.KnownFolders.DocumentsLibrary.Path;
		var unzipLocation = Application.persistentDataPath;
#else
		var zipLocation = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		var unzipLocation = zipLocation;
#endif
		Debug.Log(zipLocation);
		Debug.Log(unzipLocation);
		var zip = Path.Combine(zipLocation, "DICOM.zip");
		var path = Path.Combine(unzipLocation, "DICOM");
		Debug.Log("check dir " + path);
		Debug.Log("exists=" + Directory.Exists(path));
		if (File.Exists(zip) && !Directory.Exists(path))
		{
			Debug.Log("unzipping..");
			System.IO.Compression.ZipFile.ExtractToDirectory(zip, unzipLocation);
			Debug.Log("unzip done!");
		}
		var offset = 0;
		directoryMap = new Dictionary<GameObject, DicomDirectoryRecord>();
		rootDirectoryMap = new Dictionary<DicomDirectoryRecord, string>();
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
			offset += 1;
		}
		recognizer = new GestureRecognizer();
		recognizer.TappedEvent += Recognizer_TappedEvent;
		recognizer.StartCapturingGestures();
	}

	private void Recognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
	{
		RaycastHit hit;
		if (Physics.Raycast(headRay, out hit))
		{
			Debug.Log("tap " + hit.collider.name);
			OpenDirectory(hit.collider.gameObject);
		}
	}

	void OpenDirectory(GameObject go)
	{
		var record = directoryMap[go];
		var rootDirectory = rootDirectoryMap[record];
		var offset = 0;
		foreach (var subRecord in record.LowerLevelDirectoryRecordCollection)
		{
			rootDirectoryMap[subRecord] = rootDirectory;
			var desc = "";
			if (subRecord.DirectoryRecordType == "STUDY")
			{
				var studyDate = subRecord.Get<string>(DicomTag.StudyDate, "no study date");
				var studyDesc = subRecord.Get<string>(DicomTag.StudyDescription, "no desc");
				desc = "Study: " + studyDate + "\n" + studyDesc;
			} else if (subRecord.DirectoryRecordType == "SERIES")
			{
				var modality = subRecord.Get<string>(DicomTag.Modality, "no modality");
				var seriesDesc = subRecord.Get<string>(DicomTag.SeriesDescription, "no modality");
				desc = "Series: " + modality + "\n" + seriesDesc;
			} else if (subRecord.DirectoryRecordType == "IMAGE") {
				desc = "Image: " + subRecord.Get<string>(DicomTag.InstanceNumber);
			}
			var tex = GetImageForRecord(subRecord);
			var quad = Instantiate(quadPrefab, go.transform);
			quad.GetComponent<Renderer>().material.mainTexture = tex;
			quad.transform.localPosition += new Vector3(offset, -2, 0);
			quad.transform.Find("Canvas").Find("title").GetComponent<Text>().text = desc;
			quad.name = desc;
			directoryMap[quad] = subRecord;
			offset += 1;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0))
		{
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				Debug.Log("click " + hit.collider.name);
				OpenDirectory(hit.collider.gameObject);
			}
			float distance_to_screen = Camera.main.WorldToScreenPoint(transform.position).z;
			offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
		}
		if (Input.GetMouseButton(0))
		{
			float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
			transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen)) + offset;
		}
	}
}
