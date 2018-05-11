using Dicom;
using Dicom.Media;
using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpenSeriesHandler : MonoBehaviour {

	public DicomDirectoryRecord record;
	public LoadDICOM loadDicomInstance;
	private Renderer renderer;
	private int previousValue;

	// Use this for initialization
	void Start () {
		renderer = gameObject.GetComponent<Renderer>();
	}
	
	public void ButtonPush(string button)
	{
		if (button == "3D")
		{
			Int3 size;
			var startTime = Time.realtimeSinceStartup;
			var vol = DICOMSeriesToVolume(record, out size);
			var volumeSizePow2 = MathExtensions.PowerOfTwoGreaterThanOrEqualTo(size);
			var tex3D = VolumeTextureUtils.BuildTexture(vol, size, volumeSizePow2);
			Debug.Log("created volume in " + (Time.realtimeSinceStartup - startTime) + "s");
			var volInfo = VolumeInformation.CreateInstance<VolumeInformation>();
			volInfo.BakedTexture = tex3D;
			volInfo.Size = size;

			var vc = transform.Find("Volume Cube");
			var volControl = vc.GetComponent<VolumeController>();
			volControl.VolumeInfo = volInfo;
			vc.gameObject.SetActive(true);
			vc.transform.Find("Slicing Plane Controller").GetComponent<SlicingPlaneController>().ThickSliceMaterial.SetTexture("_VolTex", tex3D);
			GetComponent<MeshRenderer>().enabled = false;
		}
		else if (button == "2D")
		{
			GetComponent<MeshRenderer>().enabled = true;
			transform.Find("Volume Cube").gameObject.SetActive(false);
		}
	}

	public void SliderChange(float newValue)
	{
		int newValueInt = (int)newValue;
		if (newValueInt != previousValue)
		{
			var tex = loadDicomInstance.GetTexture2DForRecord(record, newValueInt);
			renderer.material.mainTexture = tex;
			previousValue = newValueInt;
		}
	}

	public byte[] DICOMSeriesToVolume(DicomDirectoryRecord series, out Int3 size)
	{
		var img = loadDicomInstance.GetImageForRecord(series);
		size = new Int3(img.Width, img.Height, series.LowerLevelDirectoryRecordCollection.Count());
		var voxels = new VolumeBuffer<Color32>(size);

		var tex = new Texture2D(2, 2);

		int z = 0;

		var instanceNumbers = series.LowerLevelDirectoryRecordCollection.Select(x => x.Get<int>(DicomTag.InstanceNumber)).OrderBy(x => x).ToArray();

		foreach (var frame in instanceNumbers)
		{
			tex = loadDicomInstance.GetTexture2DForRecord(series, frame);
			var fromPixels = tex.GetPixels32();
			for (var y = 0; y < size.y; ++y)
			{
				for (var x = 0; x < size.x; ++x)
				{
					var from = fromPixels[x + (y * size.x)];
					voxels.SetVoxel(new Int3(x, y, z), from);
				}
			}
			++z;
		}

		voxels.ClearEdges(new Color32(0, 0, 0, 0));
		return VolumeTextureUtils.Color32ArrayToByteArray(voxels.DataArray);
	}
}
