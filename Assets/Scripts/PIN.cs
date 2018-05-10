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
		try
		{
			correctPin = File.ReadAllText(Path.Combine(Application.persistentDataPath, "pin.txt"));
		}
		catch
		{
			feedback.text = "pin.txt missing!";
		}
	}

	public void OnClick(string digit)
	{
		currentEntered += digit;
		feedback.text = "Entered: " + currentEntered;
		if (currentEntered.Length == 4)
		{
			if (currentEntered == correctPin)
			{
				feedback.text += " Correct! - Loading";
				StartCoroutine(LoadSceneAsync());
			}
			else
			{
				feedback.text += " Incorrect";
				currentEntered = "";
			}
		}
	}

	IEnumerator LoadSceneAsync()
	{
		// The Application loads the Scene in the background as the current Scene runs.
		// This is particularly good for creating loading screens.
		// You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
		// a sceneBuildIndex of 1 as shown in Build Settings.

		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("main");

		// Wait until the asynchronous scene fully loads
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}
}
