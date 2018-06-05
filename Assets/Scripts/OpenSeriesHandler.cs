using Dicom;
using Dicom.Media;
using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class OpenSeriesHandler : MonoBehaviour {

	public DicomDirectoryRecord record;
	public LoadDICOM loadDicomInstance;
	public bool useCache = true;
	private Renderer renderer;
	private Renderer vcRenderer;
	public int frame;
	private int resolution = 2;
	public bool is3D = false;

	private Vector3 min = Vector3.zero;
	private Vector3 max = Vector3.one;

	// Use this for initialization
	void Awake () {
		renderer = gameObject.GetComponent<Renderer>();
		vcRenderer = transform.Find("Volume Cube").GetComponent<Renderer>();
	}
	
	public void ButtonPush(string button)
	{
		if (button == "3D")
		{
			transform.Find("zstack slider").gameObject.SetActive(false);
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
				vcRenderer.material.SetTexture("_Volume", tex3D);
			}
			renderer.enabled = false;
			is3D = true;
		}
		else if (button == "2D")
		{
			renderer.enabled = true;
			transform.Find("zstack slider").gameObject.SetActive(true);
			transform.Find("Volume Cube").gameObject.SetActive(false);
			is3D = false;
		}
	}

	public void SliderChange(float newValue)
	{
		var slider = EventSystem.current.currentSelectedGameObject.name;
		switch (slider)
		{
			case "zstack slider": // 2D slice change
				int newValueInt = (int)newValue;
				if (newValueInt != frame)
				{
					var tex = loadDicomInstance.GetTexture2DForRecord(record, newValueInt);
					renderer.material.mainTexture = tex;
					frame = newValueInt;
				}
				return;
			// 3D volume settings
			case "intensity":
				vcRenderer.material.SetFloat("_Intensity", newValue);
				return;
			case "threshold":
				vcRenderer.material.SetFloat("_Threshold", newValue);
				return;
			case "xmin":
				min.x = newValue;
				break;
			case "xmax":
				max.x = newValue;
				break;
			case "ymin":
				min.y = newValue;
				break;
			case "ymax":
				max.y = newValue;
				break;
			case "zmin":
				min.z = newValue;
				break;
			case "zmax":
				max.z = newValue;
				break;
		}
		vcRenderer.material.SetVector("_SliceMin", min);
		vcRenderer.material.SetVector("_SliceMax", max);
		Debug.Log("slicing to " + min + "-" + max);
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
