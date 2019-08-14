using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine.UI;
using HoloToolkit.Unity.SharingWithUNET;
using UnityEngine.EventSystems;

public class Genomics : MonoBehaviour
{

    public List<GameObject> lesions;
	public List<GameObject> tumours;

	public List<GameObject> mutationLabels;
	public GameObject labelPrefab;

	public List<GameObject> groupButtons;
	public GameObject toggleLabelsButton;

	private Dictionary<string, GameObject> lesionsNamed = new Dictionary<string, GameObject>();
	private Dictionary<string, GameObject> tumoursNamed = new Dictionary<string, GameObject>();
	private Dictionary<int, List<GameObject>> groups = new Dictionary<int, List<GameObject>>();
	private List<Color32> groupColors = new List<Color32>() { new Color32(237, 125, 49, 200), new Color32(214, 33, 33, 200) , new Color32(0, 176, 240, 200) , new Color32(0, 176, 80, 200) , new Color32(146, 208, 80, 200), new Color32(151, 81, 203, 200) };

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
			for (var colIndex = 2; colIndex <= dataRow.Count-1; colIndex++)
			{
				int group;
				
				bool res = int.TryParse(dataRow[colIndex].ToString(), out group);
				if (res == true && group >= 1)
				{
					SortGroups(colIndex, group);
					
					var groupText = mutationLabels[group - 1].transform.GetChild(0).GetComponent<Text>();
					var mutationName = csv[rowIndex][0].ToString();
					
					if (!groupText.text.Contains(mutationName))
					{
						groupText.text = groupText.text + " " + mutationName;
					}
				}
			}
		}

		groupButtons[0].GetComponent<Toggle>().isOn = true;
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
								Debug.Log("Add to existing group" + lesion.Value + " " + lesionName);
								list.Add(lesion.Value);
							}
						}
					}
					else {
						Debug.Log("Add to new group" + lesion.Value + " " + lesionName);
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

	public void ToggleGenomicsGroup(int groupNumber) {
		gameObject.GetComponent<SyncGenomicsUNET>().ToggleGroup(groupNumber);
	}

	public void SyncToggleGroup(int groupNumber) {
		SetColor(groupNumber);
		SetMutationLabels(groupNumber);
		
		groupButtons[groupNumber - 1].GetComponent<Toggle>().isOn = true;
	}

	public void ToggleLabels(bool toggle) {
		gameObject.GetComponent<SyncGenomicsUNET>().ToggleLabels(toggle);
	}

	public void SyncToggleLabels(bool toggle) {
		if (toggle)
		{
			foreach (var label in textLabels)
			{
				label.SetActive(true);
			}
		}
		else
		{
			foreach (var label in textLabels)
			{
				label.SetActive(false);
			}
		}
		toggleLabelsButton.GetComponent<Toggle>().isOn = toggle;
	}

	public List<GameObject> CreateLabels()
	{
		List<GameObject> tl = new List<GameObject>();

		foreach (var lesion in lesionsNamed) {
			
			Vector3 center = lesion.Value.transform.GetChild(0).GetComponent<MeshRenderer>().bounds.center;
			GameObject textLabel = Instantiate(labelPrefab, lesion.Value.transform, true);
			
			textLabel.transform.position = center;
			textLabel.transform.GetChild(0).GetComponent<Text>().text = lesion.Key.Split('_')[0];
			tl.Add(textLabel);
		}
		return tl;
	}

	private void SetColor(int groupNumber) {
		
		List<GameObject> lesionGroup;
		bool hasLesions = groups.TryGetValue(groupNumber, out lesionGroup);
		
		foreach (var lesion in lesions) {
			lesion.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", groupColors[0]);
		}

		Color32 colorValue = new Color32();
		colorValue = groupColors[groupNumber - 1];
		
		foreach (var lesion in lesionGroup) {
			Debug.Log(lesion);
			lesion.transform.GetChild(0).GetComponent<Renderer>().material.SetColor("_Color", colorValue);
		}
	}


	private List<List<string>> readCSV()
	{
		string path = @Path.Combine(Application.persistentDataPath, "Genomics/Genomics.csv");
        Debug.Log(path);
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