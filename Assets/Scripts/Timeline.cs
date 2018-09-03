using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Timeline : MonoBehaviour {

    public GameObject tumours_2007_05_01;
    public GameObject tumours_2015_02_11;
    public GameObject tumours_2015_04_28;
    public GameObject tumours_2015_08_03;
    public GameObject tumours_2016_03_31;
    public GameObject tumours_2016_09_01;

    public GameObject lesions_2016_03_31;
    public GameObject lesions_2016_09_01;

    public GameObject sliderLabel;

    // Use this for initialization
    void Start () {
        sliderLabel.GetComponent<TextMesh>().text = "01.05.2007";
    }

    public void SliderChange(float newValue)
    {
        var date = Mathf.RoundToInt(newValue);

        switch (date)
        {
            case 0:
                tumours_2007_05_01.SetActive(true);
                tumours_2015_02_11.SetActive(false);

                sliderLabel.GetComponent<TextMesh>().text = "01.05.2007";
                break;
            case 1:
                tumours_2007_05_01.SetActive(false);
                tumours_2015_02_11.SetActive(true);
                tumours_2015_04_28.SetActive(false);

                sliderLabel.GetComponent<TextMesh>().text = "11.02.2015";
                break;
            case 2:
                tumours_2015_02_11.SetActive(false);
                tumours_2015_04_28.SetActive(true);
                tumours_2015_08_03.SetActive(false);

                sliderLabel.GetComponent<TextMesh>().text = "28.04.2015";
                break;
            case 3:
                tumours_2015_04_28.SetActive(false);
                tumours_2015_08_03.SetActive(true);
                tumours_2016_03_31.SetActive(false);

                lesions_2016_03_31.SetActive(false);
                sliderLabel.GetComponent<TextMesh>().text = "03.08.2015";
                break;
            case 4:
                tumours_2015_08_03.SetActive(false);
                tumours_2016_03_31.SetActive(true);
                tumours_2016_09_01.SetActive(false);

                lesions_2016_03_31.SetActive(true);
                lesions_2016_09_01.SetActive(false);

                sliderLabel.GetComponent<TextMesh>().text = "31.03.2016";
                break;
            case 5:
                tumours_2016_03_31.SetActive(false);
                tumours_2016_09_01.SetActive(true);

                lesions_2016_03_31.SetActive(false);
                lesions_2016_09_01.SetActive(true);

                sliderLabel.GetComponent<TextMesh>().text = "01.09.2016";
                break;
            default: break;
        }
    }

}
