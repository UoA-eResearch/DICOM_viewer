using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace HoloToolkit.Unity.SharingWithUNET
{
	public class SyncGenomicsUNET : NetworkBehaviour
	{
		private int lastGroup = 0;
		private bool lastLabelState;

		public void ToggleGroup(int groupNumber)
		{
			if (lastGroup != groupNumber) {
				PlayerController.Instance.SendToggleGroup(gameObject, groupNumber);
				lastGroup = groupNumber;
			}
		}


		[ClientRpc(channel = Channels.DefaultUnreliable)]
		public void RpcToggleGroup(int groupNumber)
		{
			GetComponent<Genomics>().SyncToggleGroup(groupNumber);
			
		}


		public void ToggleLabels(bool toggle)
		{
			if (lastLabelState != toggle) {
				PlayerController.Instance.SendToggleLabels(gameObject, toggle);
				lastLabelState = toggle;
			}
		}


		[ClientRpc(channel = Channels.DefaultUnreliable)]
		public void RpcToggleLabels(bool toggle)
		{
			GetComponent<Genomics>().SyncToggleLabels(toggle);

		}


		public void Start()
		{
			lastLabelState = GetComponent<Genomics>().toggleLabelsButton.GetComponent<Toggle>().isOn;
		}
	}
}