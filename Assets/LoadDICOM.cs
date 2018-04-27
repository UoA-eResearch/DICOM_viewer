using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dicom.Imaging;
using System.IO;
using Dicom.Media;
using Dicom;
using System;

public class LoadDICOM : MonoBehaviour {

	public GameObject quadPrefab;

	void PrintTagsForRecord(DicomDirectoryRecord record)
	{
		foreach (var field in typeof(DicomTag).GetFields())
		{
			try
			{
				Debug.Log(field.Name + ":" + string.Join(",", record.Get<string[]>((DicomTag)field.GetValue(null))));
			}
			catch (Exception)
			{
			}
		}
	}

	Texture2D DicomToTex2D(DicomImage image)
	{
		var pixels = image.PixelData;
		var bytes = pixels.GetFrame(0).Data;
		var tex = new Texture2D(pixels.Height, pixels.Width);
		for (int i = 0; i < bytes.Length; i++)
		{
			int intensity = bytes[i];
			var x = i % image.Width;
			var y = i / image.Width;
			tex.SetPixel(x, y, new Color(intensity / 255f, intensity / 255f, intensity / 255f));
		}

		return tex;
	}

	// Use this for initialization
	void Start () {
		var dict = new DicomDictionary();
		dict.Load(Application.dataPath + "/StreamingAssets/Dictionaries/DICOM Dictionary.xml", DicomDictionaryFormat.XML);
		DicomDictionary.Default = dict;
		var root = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
		var path = Path.Combine(root, "DICOM");
		foreach (var directory in Directory.GetDirectories(path))
		{
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
						foreach (var imageRecord in seriesRecord.LowerLevelDirectoryRecordCollection)
						{
							var filename = Path.Combine(imageRecord.Get<string[]>(DicomTag.ReferencedFileID));
							var absoluteFilename = Path.Combine(directory, filename);
							var img = new DicomImage(absoluteFilename);
							var tex = DicomToTex2D(img);
							var quad = Instantiate(quadPrefab, transform);
							quad.GetComponent<Renderer>().material.mainTexture = tex;
							return;
						}
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
