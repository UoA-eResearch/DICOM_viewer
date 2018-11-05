using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace HoloToolkit.Unity.SharingWithUNET
{
	public class SyncTimelineUNET : NetworkBehaviour
	{
		private bool samplingSitesActive;
		private int previousTime;

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


		public void TimeChangeEvent(int currentTime)
		{
			if (previousTime != currentTime)
			{
				PlayerController.Instance.SendTimeChangeEvent(gameObject, currentTime);
				previousTime = currentTime;
			}
		}


		[ClientRpc(channel = Channels.DefaultUnreliable)]
		public void RpcTimeChangeEvent(int currentTime)
		{
			GetComponent<Timeline>().SyncTimeEvent(currentTime);
		}


		public void Start()
		{
			samplingSitesActive = GetComponent<Timeline>().toggleSamplingSitesButton.GetComponent<Toggle>().isOn;
		}
	}
}