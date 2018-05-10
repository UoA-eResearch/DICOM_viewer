using Dicom.Media;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenSeriesHandler : MonoBehaviour {

	public DicomDirectoryRecord record;
	public LoadDICOM loadDicomInstance;
	private Renderer renderer;

	// Use this for initialization
	void Start () {
		renderer = gameObject.GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SliderChange(float newValue)
	{
		var tex = loadDicomInstance.GetImageForRecord(record, (int)newValue);
		renderer.material.mainTexture = tex;
	}
}
