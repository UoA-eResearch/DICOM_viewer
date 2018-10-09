using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HoloToolkit.Unity.SharingWithUNET
{
	public class SyncLocalTransformUNET : NetworkBehaviour, IManipulationHandler
	{
		private Vector3 lastPos;
		private Quaternion lastRot;
		private bool isMoving = false;

		IEnumerator SyncTransform()
		{
			while (true)
			{
				if (isMoving && (transform.localPosition != lastPos || transform.localRotation != lastRot))
				{
					PlayerController.Instance.SendSharedTransform(gameObject, transform.localPosition, transform.localRotation);
					lastPos = transform.localPosition;
					lastRot = transform.localRotation;
				}
				yield return new WaitForSeconds(1/30f);
			}
		}

		public void Start()
		{
			lastPos = transform.localPosition;
			lastRot = transform.localRotation;
			if (!isServer)
			{
				StartCoroutine(SyncTransform());
			}
		}

		public void OnManipulationStarted(ManipulationEventData eventData)
		{
			if (eventData.selectedObject == gameObject)
			{
				isMoving = true;
			}
		}

		public void OnManipulationUpdated(ManipulationEventData eventData)
		{
		}

		public void OnManipulationCompleted(ManipulationEventData eventData)
		{
			isMoving = false;
		}

		public void OnManipulationCanceled(ManipulationEventData eventData)
		{
			isMoving = false;
		}
	}
}