using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine.UI;

public class Genomics : MonoBehaviour {

    public List<GameObject> lesions;
	public List<GameObject> tumours;

	public List<GameObject> groupLabels;
	public GameObject labelPrefab;

	private Dictionary<string, GameObject> lesionsNamed = new Dictionary<string, GameObject>();
	private Dictionary<string, GameObject> tumoursNamed = new Dictionary<string, GameObject>();
	private Dictionary<int, List<GameObject>> groups = new Dictionary<int, List<GameObject>>();
	private List<Color32> groupColors = new List<Color32>() { new Color32(92, 255, 248, 133),  new Color32(233, 159, 0, 200), new Color32(0, 99, 169, 200) , new Color32(135, 0, 255, 200) , new Color32(0, 122, 16, 200) , new Color32(174, 0, 0, 200) };

	private List<List<string>> csv;

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

		
		csv = readCSV();
		
		for(var rowIndex = 0; rowIndex <= csv.Count-1; rowIndex++)
		{
			var dataRow = csv[rowIndex];

			for (var colIndex = 5; colIndex < dataRow.Count-1; colIndex++)
			{
				int group;
				
				bool res = int.TryParse(dataRow[colIndex].ToString(), out group);
				if (res == true && group >= 1)
				{
					SortGroups(colIndex, group);
					
				}
				
			}
		}
		SetColor(1, true);
		SetColor(2, true);
		SetColor(3, true);
		SetColor(4, true);
		SetColor(5, true);

		toggleLabels(true);
	}


	private void SortGroups(int colIndex, int group) {

		var lesionName = csv[1][colIndex].ToString();
		
		Text groupText = groupLabels[group - 1].GetComponent<Text>();
		if (!groupText.text.Contains(lesionName)){
			groupText.text = groupText.text + " " + lesionName;
		}

		foreach (var lesion in lesionsNamed)
		{
			if (lesion.Key.Contains(lesionName))
			{
				StringBuilder sb = new StringBuilder(lesion.Key);
				var dicomFileName = sb.Remove(0, lesionName.ToCharArray().Length + 1).ToString();
				
				if (tumoursNamed.ContainsKey(dicomFileName))
				{
					var tumour = tumoursNamed[dicomFileName.ToString()];

					if (groups.ContainsKey(group))
					{
						List<GameObject> list;
						bool cont = groups.TryGetValue(group, out list);
						
						if (cont == true) {
							if (!list.Contains(lesion.Value)) {
								list.Add(lesion.Value);
							}
						}
					}
					else {
						groups.Add(group, new List<GameObject>() { lesion.Value});
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

	public void toggleLabels(bool toggle)
	{
		foreach (var lesion in lesionsNamed) {

			GameObject textLabel = Instantiate(labelPrefab, lesion.Value.transform, true);
			textLabel.transform.localPosition = new Vector3(0, 0, 0);
			textLabel.transform.GetChild(0).GetComponent<Text>().text = lesion.Key.Split('_')[0];
			//lesion.Value.transform.GetChild(0).gameObject.AddComponent<Text>().text = lesion.Key.Split('_')[0];
			
		}
	}

	private void SetColor(int groupNumber, bool chosen) {
		
		List<GameObject> lesions;
		var boolVal = groups.TryGetValue(groupNumber, out lesions);

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

	private List<List<string>> readCSV()
	{
		string path = @Path.Combine(Application.persistentDataPath, "Genomics/Genomics.csv");

		List<List<string>> CSV = new List<List<string>>();

		using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
		{
			using (StreamReader sr = new StreamReader(stream))
			{
				string[] headers = sr.ReadLine().Split(',');
				CSV.Add(headers.ToList());
				while (!sr.EndOfStream)
				{
					string[] rows = sr.ReadLine().Split(',');
					CSV.Add(rows.ToList());
				}
			}
		}
		return CSV;
	}

}