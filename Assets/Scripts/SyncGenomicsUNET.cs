using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.InputModule.Utilities.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HoloToolkit.Unity.SharingWithUNET
{
	public class SyncGenomicsUNET : NetworkBehaviour
	{
		private int lastGroup = 0;

		public void ToggleGroup(int groupNumber, bool toggle)
		{
			if (lastGroup != groupNumber) {
				PlayerController.Instance.SendToggleGroup(gameObject, groupNumber, toggle);
				lastGroup = groupNumber;
			}
		}


		[ClientRpc(channel = Channels.DefaultUnreliable)]
		public void RpcToggleGroup(int groupNumber, bool toggle)
		{
			GetComponent<Genomics>().SyncToggleGroup(groupNumber, toggle);
			
		}

		public void Start()
		{
			
		}
	}
}