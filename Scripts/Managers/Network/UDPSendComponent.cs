// #define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Unity Component Interfacing UDP Send class for UI.
	public class UDPSendComponent : NetworkAgentComponent {
		//[Header("Necessary Variables.")]
		private UDPSend udpSender;
		[Tooltip("A port for UDP communication to send to.")]
		public int remotePortOverride = 8811;
        
		// Local Variables.
		protected override string sourceName { get { return "UDP Sender Component"; } }
		// protected override SourceType sourceType { get { return SourceType.UDP; } }
        
		protected override void OnEnable() {
			base.OnEnable();
            
			PrepareSender();
		}
		protected override void SpecificTerminate() {
			this.udpSender.StopSending();
			this.udpSender.Disconnect();
		}
		protected override void InitiateHoloComponent(){
			this._holoComponent = new HoloComponent(SourceType.UDP, SourceCommunicationType.Sender, this.remotePortOverride);
		}
		public override void OnUpdateIP(){
			// if ((this.udpSender != null) && (this.udpSender.IP != this.remoteIP))
			// 	// Disconnect?
			SpecificTerminate();
			PrepareSender();
		}
		private bool PrepareSender(){
			if (!string.IsNullOrEmpty(this.remoteIP)) {// just in case
				if ((this.udpSender == null) || (this.udpSender.IP != this.remoteIP)) {
					this.udpSender = new UDPSend(this, this.remoteIP, this.remotePortOverride, _ownerName: this.sourceName);
					this.udpSender.StartSending();
				}
				return true;
			} else {
				#if DEBUGWARNING
				DebugUtilities.UniversalWarning(this.sourceName, "No server IP Found - enable Grasshopper UI Receiving Component");
				#endif
			}
			return false;
		}
        
		public void SendUI(byte[] data) {
			if (PrepareSender())
				this.udpSender.QueueUpData(data);
		}
	}
}