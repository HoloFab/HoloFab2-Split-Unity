#define DEBUG
#define DEBUGWARNING
// #undef DEBUG
// #undef DEBUGWARNING

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Unity Component Interfacing TCP Send class for UI.
	public class TCPSendComponent : MonoBehaviour {
		[Tooltip("Sending Frequency in milliseconds.")]
		public int delayTime = 180000;
		[Header("Necessary Variables.")]
		public TCPSend tcpSender = null;
		[Tooltip("A port for TCP communication to send to.")]
		public int remotePortOverride = 13131;
		[Tooltip("Received IP address of the computer.")]
		public string remoteIP = null;
		// Local Variables.
		private string sourceName = "TCP Sender Component";
        
		private ThreadInterface environmentMeshExtractor;
        
		// Network variables.
		// Stored message to avoid unnecessary traffic.
		private static string lastMessage;
        
		public void OnEnable(){
			this.environmentMeshExtractor = new ThreadInterface(SendData,
			                                                    this.delayTime);
		}
        
		public void OnDisable(){
			this.environmentMeshExtractor.Stop();
		}
        
		public void UpdateIP(string _remoteIP){
			if (this.remoteIP != _remoteIP) {
				this.remoteIP = _remoteIP;
				if (this.tcpSender != null)
					this.tcpSender.Disconnect();
				this.tcpSender = new TCPSend(this.remoteIP, this.remotePortOverride);
				this.tcpSender.Connect();
				this.environmentMeshExtractor.Start();
			}
		}
        
		private void SendData(){
			if (this.tcpSender != null) {
				List<byte[]> environmentData = ObjectManager.instance.EncodeEnvironmentMesh(out string currentMessage);
				if ((environmentData.Count > 0)
				   && (!string.IsNullOrEmpty(currentMessage))
				   && (TCPSendComponent.lastMessage != currentMessage)) {
					TCPSendComponent.lastMessage = currentMessage;
					foreach (byte[] data in environmentData)
						this.tcpSender.QueueUpData(data);
				} else {
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "Mesh is already sent or no mesh is found.");
					#endif
				}
			}
		}
		// public void SendMesh(byte[] data) {
		// 	if (data != null) {
		// 		if (!string.IsNullOrEmpty(this.remoteIP)) { // just in case
		// 			if ((this.tcpSender == null) || (this.tcpSender.remoteIP != this.remoteIP)) {
		// 				this.tcpSender = new TCPSend(this.remoteIP, this.remotePortOverride);
		// 				this.tcpSender.Connect();
		// 			}
		// 			this.tcpSender.QueueUpData(data);
		// 		} else {
		// 			#if DEBUGWARNING
		// 			DebugUtilities.UniversalWarning(this.sourceName, "No server IP Found - enable Grasshopper Mesh Receiving Component");
		// 			#endif
		// 		}
		// 	}
		// }
	}
}