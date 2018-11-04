using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace HoloToolkit.Unity.SharingWithUNET
{
	public class SyncTimelineUNET : NetworkBehaviour
	{
		private bool samplingSitesActive;

		public void ToggleSamplingSites(bool active)
		{
			if (samplingSitesActive != active)
			{
				PlayerController.Instance.SendToggleSamplingSites(gameObject, active);
				samplingSitesActive = active;
			}
		}


		[ClientRpc(channel = Channels.DefaultUnreliable)]
		public void RpcToggleSamplingSites(bool active)
		{
			GetComponent<Timeline>().toggleSamplingSitesButton.GetComponent<Toggle>().isOn = active;
		}


		public void Start()
		{
			samplingSitesActive = GetComponent<Timeline>().toggleSamplingSitesButton.GetComponent<Toggle>().isOn;
		}
	}
}