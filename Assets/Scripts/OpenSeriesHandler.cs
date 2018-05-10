using Dicom.Media;
using System.Collections;
using System.Collections.Generic;
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
		Debug.Log(button);
	}

	public void SliderChange(float newValue)
	{
		int newValueInt = (int)newValue;
		if (newValueInt != previousValue)
		{
			var tex = loadDicomInstance.GetImageForRecord(record, newValueInt);
			renderer.material.mainTexture = tex;
			previousValue = newValueInt;
		}
	}

	/*
	public static byte[] DICOMSeriesToVolume(DicomDirectoryRecord series, bool inferAlpha, out Int3 size)
	{
		var sampleImage = series.LowerLevelDirectoryRecordCollection.First();
		size = new Int3(tex.width, tex.height, images.Length);
		size = GetSizeOfVolumeFolder(folder);
		var voxels = new VolumeBuffer<Color32>(size);

		var tex = new Texture2D(2, 2);

		int z = 0;
		foreach (var imageFile in imageNames)
		{
			bool loaded = tex.LoadImage(FileSystemHelper.ReadBytesFromLocalFile(imageFile));
			if (!loaded)
			{
				Debug.LogError("Couldn't load '" + imageFile + "'...");
				return null;
			}
			var fromPixels = tex.GetPixels32();
			for (var y = 0; y < size.y; ++y)
			{
				for (var x = 0; x < size.x; ++x)
				{
					var from = fromPixels[x + (y * size.x)];
					if (inferAlpha)
					{
						from.a = (byte)Mathf.Max(from.r, from.g, from.b);
					}
					voxels.SetVoxel(new Int3(x, y, z), from);
				}
			}
			++z;
		}

		voxels.ClearEdges(new Color32(0, 0, 0, 0));
		return VolumeTextureUtils.Color32ArrayToByteArray(voxels.DataArray);
	}
	*/
}
