using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using HoloToolkit.Unity.SharingWithUNET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Position : MonoBehaviour {

	Vector3 origPosition;
	Quaternion origRotation;
	Vector3 origScale;

	public GameObject skel;
	public GameObject lockText;

	// Use this for initialization
	void Start () {
		origPosition = gameObject.transform.localPosition;
		origRotation = gameObject.transform.localRotation;
		origScale = gameObject.transform.localScale;
	}

	public void ResetPos()
	{
		gameObject.transform.localPosition = origPosition;
		gameObject.transform.localRotation = origRotation;
		gameObject.transform.localScale = origScale;
		
		gameObject.GetComponent<SyncLocalTransformUNET>().ResetTransform(origPosition, origRotation, origScale);
	}

	public void Save() {
		origPosition = gameObject.transform.localPosition;
		origRotation = gameObject.transform.localRotation;
		origScale = gameObject.transform.localScale;

		gameObject.GetComponent<SyncLocalTransformUNET>().SetSavedPosition(origPosition, origRotation, origScale);
	}

	public void Lock() {
		gameObject.GetComponent<SyncLocalTransformUNET>().LockTransform(lockText.GetComponent<Text>().text);
	}

	public void SetSavedTransform(Vector3 newPos, Quaternion newRot, Vector3 newScale) {
		origPosition = newPos;
		origRotation = newRot;
		origScale = newScale;
	}

	// Update is called once per frame
	void Update () {
		
	}
}