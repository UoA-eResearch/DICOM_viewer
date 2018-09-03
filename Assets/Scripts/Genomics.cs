using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Genomics : MonoBehaviour {

    public List<GameObject> lesions;

    private Dictionary<string, GameObject> lesionsNamed = new Dictionary<string, GameObject>();

	// Use this for initialization
	void Start () {

        foreach (GameObject lesion in lesions) {
            var split = lesion.name.Split('_');
            lesionsNamed.Add(split[1], lesion);

            
            
        }
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}