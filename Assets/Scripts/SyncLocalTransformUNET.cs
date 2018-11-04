using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HoloToolkit.Unity.SharingWithUNET
{
	[RequireComponent(typeof(TwoHandManipulatable))]
	public class SyncLocalTransformUNET : NetworkBehaviour
	{
		private Vector3 lastPos;
		private Quaternion lastRot;
		private Vector3 lastScale;

		private TwoHandManipulatable twoHandManipulatable;

		IEnumerator SyncTransform()
		{
			while (true)
			{
				if (twoHandManipulatable.currentState != ManipulationMode.None && (transform.localPosition != lastPos || transform.localRotation != lastRot || transform.localScale != lastScale))
				{
					PlayerController.Instance.SendSharedTransform(gameObject, transform.localPosition, transform.localRotation, transform.localScale);
					lastPos = transform.localPosition;
					lastRot = transform.localRotation;
					lastScale = transform.localScale;
				}
				yield return new WaitForSeconds(1/30f);
			}
		}

		[ClientRpc(channel = Channels.DefaultUnreliable)]
		public void RpcSetLocalTransform(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			if (twoHandManipulatable.currentState == ManipulationMode.None)
			{
				transform.localPosition = position;
				transform.localRotation = rotation;
				transform.localScale = scale;
			}
		}

		public void ResetTransform(Vector3 newPos, Quaternion newRot, Vector3 newScale) {
			PlayerController.Instance.SendSharedTransform(gameObject, newPos, newRot, newScale);
			lastPos = newPos;
			lastRot = newRot;
			lastScale = newScale;
		}

		public void SetSavedPosition(Vector3 newPos, Quaternion newRot, Vector3 newScale) {
			PlayerController.Instance.SendSharedSavedPosition(gameObject, transform.localPosition, transform.localRotation, transform.localScale);
		}


		[ClientRpc(channel = Channels.DefaultUnreliable)]
		public void RpcSetSavedTransform(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			GetComponent<Position>().SetSavedTransform(position, rotation, scale);
		}

		public void Start()
		{
			lastPos = transform.localPosition;
			lastRot = transform.localRotation;
			lastScale = transform.localScale;
			twoHandManipulatable = GetComponent<TwoHandManipulatable>();
			StartCoroutine(SyncTransform());
		}
	}
}