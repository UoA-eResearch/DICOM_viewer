using Dicom;
using Dicom.Media;
using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class OpenSeriesHandler : MonoBehaviour {

	public DicomDirectoryRecord record;
	public LoadDICOM loadDicomInstance;
	public bool useCache = true;
	private Renderer renderer;
	private int previousValue;
	private int resolution = 2;

	// Use this for initialization
	void Awake () {
		renderer = gameObject.GetComponent<Renderer>();
	}
	
	public void ButtonPush(string button)
	{
		if (button == "3D")
		{
			var vc = transform.Find("Volume Cube");
			vc.gameObject.SetActive(true);

			if (vc.GetComponent<Renderer>().material.GetTexture("_Volume") == null)
			{
				var id = record.Get<string>(DicomTag.SeriesInstanceUID);
				var path = Path.Combine(Application.persistentDataPath, "Volumes", id);
				byte[] vol;
				Int3 size;

				var startTime = Time.realtimeSinceStartup;

				if (File.Exists(path) && useCache) // a cached volume for this series exists
				{
					vol = File.ReadAllBytes(path);
					size = GetSizeForRecord(record);
				}
				else
				{
					vol = DICOMSeriesToVolume(record, out size);
					File.WriteAllBytes(path, vol);
				}


				var volumeSizePow2 = MathExtensions.PowerOfTwoGreaterThanOrEqualTo(size);
				var tex3D = VolumeTextureUtils.BuildTexture(vol, size, volumeSizePow2);

				Debug.Log("created volume in " + (Time.realtimeSinceStartup - startTime) + "s");
				vc.GetComponent<Renderer>().material.SetTexture("_Volume", tex3D);
			}
			renderer.enabled = false;
		}
		else if (button == "2D")
		{
			renderer.enabled = true;
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

	public Int3 GetSizeForRecord(DicomDirectoryRecord series)
	{
		var img = loadDicomInstance.GetImageForRecord(series);
		return new Int3(img.Width / resolution, img.Height / resolution, series.LowerLevelDirectoryRecordCollection.Count());
	}

	public byte[] DICOMSeriesToVolume(DicomDirectoryRecord series, out Int3 size)
	{
		size = GetSizeForRecord(series);
		var voxels = new VolumeBuffer<Color32>(size);

		var tex = new Texture2D(2, 2);

		var instanceNumbers = series.LowerLevelDirectoryRecordCollection.Select(x => x.Get<int>(DicomTag.InstanceNumber)).OrderBy(x => x).ToArray();

		for (var z = 0; z < instanceNumbers.Length;  z++)
		{
			var frame = instanceNumbers[z];
			tex = loadDicomInstance.GetTexture2DForRecord(series, frame);
			var fromPixels = tex.GetPixels32();
			for (var y = 0; y < size.y; y++)
			{
				for (var x = 0; x < size.x; x++)
				{
					var from = fromPixels[x * resolution + ((size.y - 1 - y) * size.x) * resolution];
					voxels.SetVoxel(new Int3(x, y, z), from);
				}
			}
		}

		voxels.ClearEdges(new Color32(0, 0, 0, 0));
		return VolumeTextureUtils.Color32ArrayToByteArray(voxels.DataArray);
	}
}
