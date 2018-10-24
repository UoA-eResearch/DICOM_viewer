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

	public List<GameObject> mutationLabels;
	public GameObject labelPrefab;

	private Dictionary<string, GameObject> lesionsNamed = new Dictionary<string, GameObject>();
	private Dictionary<string, GameObject> tumoursNamed = new Dictionary<string, GameObject>();
	private Dictionary<int, List<GameObject>> groups = new Dictionary<int, List<GameObject>>();
	private List<Color32> groupColors = new List<Color32>() { new Color32(233, 159, 0, 200), new Color32(0, 99, 169, 200) , new Color32(135, 0, 255, 200) , new Color32(0, 122, 16, 200) , new Color32(174, 0, 0, 200) };

	private List<List<string>> csv;

	public float FadeDuration = 10f;
	public Color Color1 = Color.gray;
	public Color Color2 = Color.white;

	private int colourIndex = 0;
	private Color startColor;
	private Color endColor;
	private float lastColorChangeTime;

	private Material material;

	private List<GameObject> textLabels;


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

		textLabels = CreateLabels();
		
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
					
					var groupText = mutationLabels[group - 1].transform.GetChild(0).GetComponent<Text>();
					var mutationName = csv[rowIndex][1].ToString();
					Debug.Log(mutationName);
					Debug.Log(groupText);
					if (!groupText.text.Contains(mutationName))
					{
						groupText.text = groupText.text + " " + mutationName;
					}
				}
			}
		}

		SetColor(1, true);
		SetMutationLabels(1);
		ToggleLabels(true);
	}


	private void SortGroups(int colIndex, int group) {

		var lesionName = csv[1][colIndex].ToString();

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

	public void SetMutationLabels(int group) {
		foreach (var label in mutationLabels) {
			if (label.name.Contains(group.ToString()))
			{
				label.SetActive(true);
			}
			else {
				label.SetActive(false);
			}
		}
	}

	public void ToggleGenomicsGroup1(bool toggle) {
		SetColor(1, toggle);
		SetMutationLabels(1);
	}

	public void ToggleGenomicsGroup2(bool toggle)
	{
		SetColor(2, toggle);
		SetMutationLabels(2);
	}

	public void ToggleGenomicsGroup3(bool toggle)
	{
		SetColor(3, toggle);
		SetMutationLabels(3);
	}

	public void ToggleGenomicsGroup4(bool toggle)
	{
		SetColor(4, toggle);
		SetMutationLabels(4);
	}

	//public void ToggleGenomicsGroup5(bool toggle)
	//{
	//	SetColor(5, toggle);
	//}

	public void ToggleLabels(bool toggle) {
		if (toggle)
		{
			foreach (var label in textLabels)
			{
				label.SetActive(true);
			}
		}
		else {
			foreach (var label in textLabels)
			{
				label.SetActive(false);
			}
		}
	}

	public List<GameObject> CreateLabels()
	{
		List<GameObject> tl = new List<GameObject>();

		foreach (var lesion in lesionsNamed) {
			Debug.Log(lesion.Value.transform.GetChild(0).GetComponent<MeshRenderer>());
			Vector3 center = lesion.Value.transform.GetChild(0).GetComponent<MeshRenderer>().bounds.center;
			GameObject textLabel = Instantiate(labelPrefab, lesion.Value.transform, true);
			
			textLabel.transform.position = center;
			textLabel.transform.GetChild(0).GetComponent<Text>().text = lesion.Key.Split('_')[0];
			tl.Add(textLabel);
		}
		return tl;
	}

	private void SetColor(int groupNumber, bool chosen) {
		
		List<GameObject> lesionGroup;
		var boolVal = groups.TryGetValue(groupNumber, out lesionGroup);

		foreach (var lesion in lesions) {
			lesion.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", groupColors[0]);
		}

		Color32 colorValue = new Color32();
		if (chosen) {
			colorValue = groupColors[groupNumber - 1];
		}
		
		foreach (var lesion in lesionGroup) {
			lesion.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", colorValue);
		}
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