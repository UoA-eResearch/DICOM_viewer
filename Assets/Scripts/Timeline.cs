using HoloToolkit.Unity.SharingWithUNET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Timeline : MonoBehaviour {

    public GameObject tumours_2007_05_01;
    public GameObject tumours_2015_04_28;
    public GameObject tumours_2015_08_03;
    public GameObject tumours_2016_03_31;
    public GameObject tumours_2016_09_01;

    public GameObject lesions_2016_09_01;

    public GameObject sliderLabel;
	public GameObject backLabel;
	public GameObject forwardLabel;

	public GameObject buttonBack;
	public GameObject buttonForward;
	public GameObject toggleSamplingSitesButton;

	private int currentState = 4;

	// Use this for initialization
	void Start () {
		SliderChange(currentState);
	}

	public void SliderChange(int state)
    {

        switch (state)
        {
            case 0:
                tumours_2007_05_01.SetActive(true);
				tumours_2015_04_28.SetActive(false);

				sliderLabel.GetComponent<Text>().text = "01.05.2007";
				backLabel.GetComponent<Text>().text = "";
				forwardLabel.GetComponent<Text>().text = "28.04.2015";

				buttonBack.SetActive(false);
				break;
            case 1:
				tumours_2007_05_01.SetActive(false);
				tumours_2015_04_28.SetActive(true);
                tumours_2015_08_03.SetActive(false);

                sliderLabel.GetComponent<Text>().text = "28.04.2015";
				backLabel.GetComponent<Text>().text = "01.05.2007";
				forwardLabel.GetComponent<Text>().text = "03.08.2015";

				buttonBack.SetActive(true);
				break;
            case 2:
                tumours_2015_04_28.SetActive(false);
                tumours_2015_08_03.SetActive(true);
                tumours_2016_03_31.SetActive(false);
				
                sliderLabel.GetComponent<Text>().text = "03.08.2015";
				backLabel.GetComponent<Text>().text = "28.04.2015";
				forwardLabel.GetComponent<Text>().text = "31.03.2016";

				break;
            case 3:
                tumours_2015_08_03.SetActive(false);
                tumours_2016_03_31.SetActive(true);
                tumours_2016_09_01.SetActive(false);

                sliderLabel.GetComponent<Text>().text = "31.03.2016";
				backLabel.GetComponent<Text>().text = "03.08.2015";
				forwardLabel.GetComponent<Text>().text = "01.09.2016";

				buttonForward.SetActive(true);
				break;
            case 4:
                tumours_2016_03_31.SetActive(false);
                tumours_2016_09_01.SetActive(true);

                sliderLabel.GetComponent<Text>().text = "01.09.2016";
				backLabel.GetComponent<Text>().text = "31.03.2016";
				forwardLabel.GetComponent<Text>().text = "";

				buttonForward.SetActive(false);
				break;
            default: break;
        }

	}

	public void SyncTimeEvent(int currentTime) {
		currentState = currentTime;
		SliderChange(currentTime);
	}

	public void SetLesionsOnOff(bool state) {
		lesions_2016_09_01.SetActive(state);
		gameObject.GetComponent<SyncTimelineUNET>().ToggleSamplingSites(state);
	}

	public void ButtonBackEvent() {
		currentState -= 1;
		SliderChange(currentState);
		gameObject.GetComponent<SyncTimelineUNET>().TimeChangeEvent(currentState);
	}

	public void ButtonForwardEvent() {
		currentState += 1;
		SliderChange(currentState);
		gameObject.GetComponent<SyncTimelineUNET>().TimeChangeEvent(currentState);
	}

}
