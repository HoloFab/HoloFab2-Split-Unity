//#define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Unity Component Interfacing UDP Receive class.
	public class UDPClientAcknowledgerComponent : NetworkAgentComponent {
		[Header("Necessary Variables.")]
		[Tooltip("A port for UDP communication to send acknowledge to.")]
		public int remotePortOverride = 8803;
        
		// Local Variables.
		protected override string sourceName { get { return "UDP System Ack Component"; } }
		// protected override SourceType sourceType { get { return SourceType.UDP; } }
		private UDPSend udpSender;
		// - last interpreted message.
		private string lastMessage = "";
        
		// Unity Functions.
		protected override void OnEnable() {
			base.OnEnable();
            
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
				DebugUtilities.UniversalWarning(this.sourceName, "No server IP Found");
				#endif
			}
			return false;
		}
		protected override void SpecificTerminate() {
			this.udpSender.StopSending();
			this.udpSender.Disconnect();
		}
		public override void OnUpdateIP(){
			// if ((this.udpSender != null) && (this.udpSender.IP != this.remoteIP))
			// 	// Disconnect?
			//SpecificTerminate();
			PrepareSender();
		}
		protected override void InitiateHoloComponent(){
			this._holoComponent = new HoloComponent(SourceType.UDP, SourceCommunicationType.Sender, this.remotePortOverride);
		}
		/////////////////////////////////////////////////////////////////////////////
		// A function responsible for decoding and reacting to received UDP data.
		public void Acknowledge(){
			byte[] data = EncodeUtilities.EncodeData("HOLOACKNOWLEDGE", NetworkManager.instance.holoState, out _);
			this.udpSender.QueueUpData(data);
		}
	}
}