using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spire.Xls;
using System.IO;
using System.Data;
using System.Text;

public class Genomics : MonoBehaviour {

    public List<GameObject> lesions;
	public List<GameObject> tumours;

	public bool group1 = false;
	public bool group2 = false;
	public bool group3 = false;

	private Dictionary<string, GameObject> lesionsNamed = new Dictionary<string, GameObject>();
	private Dictionary<string, GameObject> tumoursNamed = new Dictionary<string, GameObject>();
	private Dictionary<int, List<GameObject>> groups = new Dictionary<int, List<GameObject>>();
	private List<Color32> groupColors = new List<Color32>() { new Color32(92, 255, 248, 133),  new Color32(233, 159, 0, 200), new Color32(0, 99, 169, 200) , new Color32(135, 0, 255, 200) , new Color32(0, 122, 16, 200) , new Color32(174, 0, 0, 200) };

	private DataTable dt;

	public float FadeDuration = 10f;
	public Color Color1 = Color.gray;
	public Color Color2 = Color.white;

	private int colourIndex = 0;
	private Color startColor;
	private Color endColor;
	private float lastColorChangeTime;

	private Material material;


	// Use this for initialization
	void Start()
	{

		foreach (GameObject lesion in lesions)
		{
			var split = lesion.name.Split('_');
			string comb = string.Concat(split[split.Length - 6], "_", split[split.Length - 5], "_", split[split.Length - 4], "_", split[split.Length - 3], "_", split[split.Length - 2], "_", split[split.Length - 1]);
			if (!lesionsNamed.ContainsKey(comb)) {
				lesionsNamed.Add(comb, lesion);
			}
		}

		foreach (GameObject tumour in tumours)
		{
			var split = tumour.name.Split('_');
			string comb = string.Concat(split[split.Length - 5], "_", split[split.Length - 4], "_", split[split.Length - 3], "_", split[split.Length - 2], "_", split[split.Length - 1]);
			if (!tumoursNamed.ContainsKey(comb)){
				tumoursNamed.Add(comb, tumour);
			}
		}

		Workbook workbook = new Workbook();
		workbook.LoadFromFile(@Path.Combine(Application.persistentDataPath, "Genomics.xlsx"));
		Worksheet sheet = workbook.Worksheets[0];
		dt = sheet.ExportDataTable();
		
		for(var rowIndex = 1; rowIndex <= dt.Rows.Count-1; rowIndex++)
		{
			var dataRow = dt.Rows[rowIndex];

			for (var colIndex = 5; colIndex < dataRow.ItemArray.Length; colIndex++)
			{
				int group;
				bool res = int.TryParse(dataRow.ItemArray[colIndex].ToString(), out group);
				if (res == true && group >= 1)
				{
					SortGroups(colIndex, group);
					
				}
				
			}
		}
	}


	private void SortGroups(int colIndex, int group) {

		var lesionName = dt.Rows[0].ItemArray[colIndex].ToString();
		
		foreach (var lesion in lesionsNamed)
		{
			if (lesion.Key.Contains(lesionName))
			{
				StringBuilder sb = new StringBuilder(lesion.Key);
				var dicomFileName = sb.Remove(0, lesionName.ToCharArray().Length + 1).ToString();
				
				if (tumoursNamed.ContainsKey(dicomFileName))
				{
					
					var tumour = tumoursNamed[dicomFileName.ToString()];
					
					
					//tumour.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", groupColors[group-1]);

					if (groups.ContainsKey(group))
					{
						List<GameObject> list;
						bool cont = groups.TryGetValue(group, out list);
						
						if (cont == true) {
							if (!list.Contains(lesion.Value)) {
								list.Add(lesion.Value);
							}
							if (!list.Contains(tumour))
							{
								//list.Add(tumour);
							}
						}
					}
					else {
						groups.Add(group, new List<GameObject>() { lesion.Value});// and tumour
					}

					
				}
			}
		}
	}

	public void toggleGenomicsGroup1(bool toggle) {
		SetColor(1, toggle);
	}

	public void toggleGenomicsGroup2(bool toggle)
	{
		SetColor(2, toggle);
	}

	public void toggleGenomicsGroup3(bool toggle)
	{
		SetColor(3, toggle);
	}

	public void toggleGenomicsGroup4(bool toggle)
	{
		SetColor(4, toggle);
	}

	public void toggleGenomicsGroup5(bool toggle)
	{
		SetColor(5, toggle);
	}

	private void SetColor(int groupNumber, bool chosen) {
		
		List<GameObject> lesions;
		groups.TryGetValue(groupNumber, out lesions);

		Color32 colorValue = new Color32();
		if (chosen) {
			colorValue = groupColors[groupNumber];
		}
		else {
			colorValue = groupColors[0];
		}
		foreach (var lesion in lesions) {
			lesion.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", colorValue);
		}
	}

	// Update is called once per frame
	void Update()
	{ /*
		foreach (var group in groups)
		{
			foreach (var ent in group.Value)
			{
				material = ent.transform.GetChild(0).GetComponent<Renderer>().material;
				
				Debug.Log(colourIndex);
				if (colourIndex >= groupColors.Count - 1)
				{
					colourIndex = 0;
				}

				startColor = groupColors[colourIndex];
				endColor = groupColors[colourIndex + 1];
				colourIndex++;

				var ratio = (Time.time - lastColorChangeTime) / FadeDuration;
				ratio = Mathf.Clamp01(ratio);
				material.color = Color.Lerp(startColor, endColor, ratio);


				if (ratio == 10f)
				{
					lastColorChangeTime = Time.time;

					// Switch colors
					var temp = startColor;
					startColor = endColor;
					endColor = temp;
				}
			}
		}
		*/
	}
	
}