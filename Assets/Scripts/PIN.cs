using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PIN : MonoBehaviour {

	public Text feedback;
	private string currentEntered = "";
	private string correctPin;

	private void Start()
	{
		correctPin = File.ReadAllText(Path.Combine(Application.persistentDataPath, "pin.txt"));
	}

	public void OnClick(string digit)
	{
		currentEntered += digit;
		feedback.text = "Entered: " + currentEntered;
		if (currentEntered.Length == 4)
		{
			if (currentEntered == correctPin)
			{
				feedback.text += " Correct!";
				SceneManager.LoadScene("main");
			}
			else
			{
				feedback.text += " Incorrect";
				currentEntered = "";
			}
		}
	}
}
