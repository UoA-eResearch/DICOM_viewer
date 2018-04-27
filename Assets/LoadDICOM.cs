using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dicom.Imaging;
using System.IO;
using System.Linq;
using Dicom.Media;
using Dicom;

public class LoadDICOM : MonoBehaviour {

	public GameObject quadPrefab;

	// Use this for initialization
	void Start () {
		var dict = new DicomDictionary();
		dict.Load(Application.dataPath + "/StreamingAssets/Dictionaries/DICOM Dictionary.xml", DicomDictionaryFormat.XML);
		DicomDictionary.Default = dict;
		var path = Application.dataPath + "/StreamingAssets/DICOM";
		foreach (var directory in Directory.GetDirectories(path))
		{
			var dd = DicomDirectory.Open(directory + "/DICOMDIR");
			Debug.Log(dd.ToString());
			return;
			Debug.Log("Loading directory " + directory);
			foreach (var file in Directory.GetFiles(directory).Where(name => !name.EndsWith(".meta")))
			{
				Debug.Log("Loading file " + file);
				var image = new DicomImage(file);
				var ri = image.RenderImage();
				var tex = new Texture2D(image.Height, image.Width);
				for (int i = 0; i < ri.Pixels.Count; i++)
				{
					int intensity = ri.Pixels.Data[i];
					var x = i % image.Width;
					var y = i / image.Width;
					tex.SetPixel(x, y, new Color(intensity / 255f, intensity / 255f, intensity / 255f));
				}
				var quad = Instantiate(quadPrefab, transform);
				quad.GetComponent<Renderer>().material.mainTexture = tex;
				
				return;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
