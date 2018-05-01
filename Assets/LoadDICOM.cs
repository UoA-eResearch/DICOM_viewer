using UnityEngine;
using Dicom.Imaging;
using System.IO;
using Dicom.Media;
using Dicom;
using System.Linq;

public class LoadDICOM : MonoBehaviour {

	public GameObject quadPrefab;

	ushort[] ConvertByteArray(byte[] bytes)
	{
		var size = bytes.Length / sizeof(ushort);
		var ints = new ushort[size];
		for (var index = 0; index < size; index++)
		{
			ints[index] = System.BitConverter.ToUInt16(bytes, index * sizeof(ushort));
		}
		return ints;
	}

	Texture2D DicomToTex2D(DicomImage image)
	{
		var pixels = image.PixelData;
		var bytes = pixels.GetFrame(0).Data;
		var ushorts = ConvertByteArray(bytes);
		Debug.Log(pixels.Height + "," + pixels.Width);
		Debug.Log(ushorts.Length);
		var tex = new Texture2D(pixels.Width, pixels.Height);
		var maxIntensity = ushorts.Max();
		for (int y = 0; y < pixels.Height; y++)
		{
			for (int x = 0; x < pixels.Width; x++)
			{
				ushort intensity = ushorts[y * pixels.Width + x];
				var rescaledIntensity = intensity / maxIntensity * 255;
				//Debug.Log("intensity at " + x + "," + y + "=" + intensity);
				var color = image.GrayscaleColorMap[rescaledIntensity];
				tex.SetPixel(x, y, new Color(color.R, color.G, color.B));
			}
		}
		tex.Apply();

		return tex;
	}

	// Use this for initialization
	void Start () {
		var dict = new DicomDictionary();
		dict.Load(Application.dataPath + "/StreamingAssets/Dictionaries/DICOM Dictionary.xml", DicomDictionaryFormat.XML);
		DicomDictionary.Default = dict;
		#if !UNITY_EDITOR && UNITY_METRO
		var root = Windows.Storage.ApplicationData.Current.RoamingFolder.Path;
		#else
		var root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		#endif
		var path = Path.Combine(root, "DICOM");
		foreach (var directory in Directory.GetDirectories(path))
		{
			var offset = 0;
			Debug.Log("--DIRECTORY--" + directory);
			var dd = DicomDirectory.Open(directory + "/DICOMDIR");
			foreach (var patientRecord in dd.RootDirectoryRecordCollection)
			{
				Debug.Log("--PATIENT--");
				foreach (var studyRecord in patientRecord.LowerLevelDirectoryRecordCollection)
				{
					Debug.Log("--STUDY--");
					Debug.Log(studyRecord.Get<string>(DicomTag.StudyDate, "no study date"));
					Debug.Log(studyRecord.Get<string>(DicomTag.StudyDescription, "no desc"));
					foreach (var seriesRecord in studyRecord.LowerLevelDirectoryRecordCollection)
					{
						Debug.Log("--SERIES--");
						Debug.Log(seriesRecord.Get<string>(DicomTag.Modality, "no modality"));
						Debug.Log(seriesRecord.Get<string>(DicomTag.SeriesDescription, "no modality"));
						var imageRecord = seriesRecord.LowerLevelDirectoryRecordCollection.ToArray().First();
						var filename = Path.Combine(imageRecord.Get<string[]>(DicomTag.ReferencedFileID));
						var absoluteFilename = Path.Combine(directory, filename);
						var img = new DicomImage(absoluteFilename);
						var tex = DicomToTex2D(img);
						var quad = Instantiate(quadPrefab, transform);
						quad.GetComponent<Renderer>().material.mainTexture = tex;
						quad.transform.Translate(offset, 0, 0);
						offset += 1;
					}
					return;
				}
			}
			return;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
