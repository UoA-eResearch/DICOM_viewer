using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PIN : MonoBehaviour {

	public Text feedback;
	private string currentEntered = "";
	private string correctPin;
	public GameObject root;

	private void Start()
	{
		try
		{
			correctPin = File.ReadAllText(Path.Combine(Application.persistentDataPath, "pin.txt"));
		}
		catch
		{
			feedback.text = "pin.txt missing!";
		}
#if UNITY_EDITOR
		root.SetActive(true);
		gameObject.SetActive(false);
#endif
	}

	public void OnClick(string digit)
	{
		currentEntered += digit;
		feedback.text = "Entered: " + currentEntered;
		if (currentEntered.Length == 4)
		{
			if (currentEntered == correctPin)
			{
				feedback.text = "Entered: ";
				currentEntered = "";
				root.SetActive(true);
				gameObject.SetActive(false);
			}
			else
			{
				feedback.text += " Incorrect";
				currentEntered = "";
			}
		}
	}
}
