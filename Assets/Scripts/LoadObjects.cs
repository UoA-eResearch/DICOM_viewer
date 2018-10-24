using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class LoadObjects : MonoBehaviour {
    
    public GameObject tumours_2007_05_01;
    public GameObject tumours_2015_04_28;
    public GameObject tumours_2015_08_03;
    public GameObject tumours_2016_03_31;
    public GameObject tumours_2016_09_01;
	
    public GameObject lesions_2016_09_01;

    public Material tumourMaterial;

	// Use this for initialization
	void Start () {

        loadObjects(Directory.GetFiles(@Path.Combine(Application.persistentDataPath, "Tumours/Tumours_2016_09_01/")), tumours_2016_09_01);
        loadObjects(Directory.GetFiles(@Path.Combine(Application.persistentDataPath, "Tumours/Tumours_2016_03_31/")), tumours_2016_03_31);
        loadObjects(Directory.GetFiles(@Path.Combine(Application.persistentDataPath, "Tumours/Tumours_2015_08_03/")), tumours_2015_08_03);
        loadObjects(Directory.GetFiles(@Path.Combine(Application.persistentDataPath, "Tumours/Tumours_2015_04_28/")), tumours_2015_04_28);
        loadObjects(Directory.GetFiles(@Path.Combine(Application.persistentDataPath, "Tumours/Tumours_2007_05_01/")), tumours_2007_05_01);
        
        //loadObjects(Directory.GetFiles(@Path.Combine(Application.persistentDataPath, "Lesions/Lesions_2016_09_01/")), lesions_2016_09_01);
    }

    private void loadObjects(string[] pathArray, GameObject parentObject) {
        
        foreach (string file in pathArray)
        {
            if (file.EndsWith(".obj"))
            {
                GameObject obj = OBJLoader.LoadOBJFile(file);
                obj.transform.parent = parentObject.transform;
                obj.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                obj.GetComponentInChildren<Renderer>().material = tumourMaterial;
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
