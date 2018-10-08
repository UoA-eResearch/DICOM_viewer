using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PIN : MonoBehaviour {

	public Text feedback;
	private string currentEntered = "";
	private string correctPin;
	public GameObject next;

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
		next.transform.localScale = Vector3.one;
		gameObject.SetActive(false);
#else
		next.transform.localScale = Vector3.zero;
		gameObject.SetActive(true);
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
				next.transform.localScale = Vector3.one;
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
