//#define DEBUG
//#define DEBUG2
#define DEBUGWARNING
#undef DEBUG
#undef DEBUG2
//#undef DEBUGWARNING

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Send part Unity Component Interfacing TCP Agent class.
	public partial class TCPAgentComponent {
		[Tooltip("Sending Frequency in milliseconds.")]
		public int delayTime = 180000;
		// [Tooltip("A port for TCP communication to send to.")]
		// public int remotePortOverride = 13131; // NOT NECESSARY since we accept connection here not send it
		// Local Variables.
		// private TaskInterface environmentMeshExtractor;
		// Stored message to avoid unnecessary traffic.
		private static string lastMessageSend;
        
		private void Send_Enable() {
			// this.environmentMeshExtractor = new TaskInterface(SendData,
			//                                                   this.delayTime);
		}
        
		private void Send_SpecificTerminate() {
			// this.environmentMeshExtractor.Stop();
			this.tcpAgent.StopReceiving();
		}
        
		public override void OnUpdateIP(){
			// if (this.tcpAgent != null)
			// 	this.tcpAgent.Disconnect();
			// this.tcpAgent = new TCPAgent(this, this.remoteIP, this.remotePortOverride);
			// this.tcpAgent.Connect();
			// this.tcpAgent.StartSending();
			// this.environmentMeshExtractor.Start();
		}
        
		// private void SendData(){
		// 	if (this.tcpAgent != null) {
		// 		List<byte[]> environmentData = ObjectManager.instance.EncodeEnvironmentMesh(out string currentMessage);
		// 		if ((environmentData.Count > 0)
		// 		   && (!string.IsNullOrEmpty(currentMessage))
		// 		   && (TCPSendComponent.lastMessageSend != currentMessage)) {
		// 			TCPSendComponent.lastMessageSend = currentMessage;
		// 			foreach (byte[] data in environmentData)
		// 				this.tcpAgent.QueueUpData(data);
		// 		} else {
		// 			#if DEBUG
		// 			DebugUtilities.UniversalDebug(this.sourceName, "Mesh is already sent or no mesh is found.");
		// 			#endif
		// 		}
		// 	}
		// }
		// // public void SendMesh(byte[] data) {
		// // 	if (data != null) {
		// // 		if (!string.IsNullOrEmpty(this.remoteIP)) { // just in case
		// // 			if ((this.tcpAgent == null) || (this.tcpAgent.remoteIP != this.remoteIP)) {
		// // 				this.tcpAgent = new TCPSend(this.remoteIP, this.remotePortOverride);
		// // 				this.tcpAgent.Connect();
		// // 			}
		// // 			this.tcpAgent.QueueUpData(data);
		// // 		} else {
		// // 			#if DEBUGWARNING
		// // 			DebugUtilities.UniversalWarning(this.sourceName, "No server IP Found - enable Grasshopper Mesh Receiving Component");
		// // 			#endif
		// // 		}
		// // 	}
		// // }
	}
}