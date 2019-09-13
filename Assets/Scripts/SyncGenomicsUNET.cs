using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace HoloToolkit.Unity.SharingWithUNET
{
	public class SyncGenomicsUNET : NetworkBehaviour
	{
		private bool lastLabelState;
        private bool lastSequState;

		public void ToggleGroup(int groupNumber)
		{
            PlayerController.Instance.SendToggleGroup(gameObject, groupNumber);
        }


		[ClientRpc(channel = Channels.DefaultUnreliable)]
		public void RpcToggleGroup(int groupNumber)
		{
			GetComponent<Genomics>().SyncToggleGroup(groupNumber);
			
		}


		public void ToggleLabels(bool toggle)
		{
            PlayerController.Instance.SendToggleLabels(gameObject, toggle);
        }


		[ClientRpc(channel = Channels.DefaultUnreliable)]
		public void RpcToggleLabels(bool toggle)
		{
			GetComponent<Genomics>().SyncToggleLabels(toggle);

		}

        public void ToggleSequential(bool sequ)
        {
            if (lastSequState != sequ)
            {
                PlayerController.Instance.SendToggleSequential(gameObject, sequ);
                lastSequState = sequ;
            }
        }


        [ClientRpc(channel = Channels.DefaultUnreliable)]
        public void RpcToggleSequential(bool sequ)
        {
            GetComponent<Genomics>().SyncToggleSequential(sequ);

        }
        
	}
}