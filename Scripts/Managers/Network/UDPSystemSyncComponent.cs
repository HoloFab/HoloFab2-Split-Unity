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
	public class UDPSystemSyncComponent : NetworkReceiverAgentComponent {
		[Header("Necessary Variables.")]
		[Tooltip("A port for UDP communication to listen on.")]
		public int localPortOverride = 8802;
        
		private UDPReceive udpReceiver;
		// Local Variables.
		protected override string sourceName { get { return "UDP System Sync Component"; } }
		// protected override SourceType sourceType { get { return SourceType.UDP; } }
		// // - IP Address received.
		// public static bool flagUICommunicationStarted = false;
		protected override Dictionary<string, Interpreter> validInterpreters {
			get {
				return new Dictionary<string, Interpreter>(){
						   {"HOLOSTATE", InterpreteHoloState},
						   {"HOLOTERMINATED", InterpreteHoloTerminate}
				};
			}
		}
		// Unity Functions.
		protected override void OnEnable() {
			base.OnEnable();
            
			this.udpReceiver = new UDPReceive(this, this.localPortOverride, _ownerName: this.sourceName);
			this.udpReceiver.Connect();
			this.udpReceiver.OnDataReceived += OnDataReceived;
			this.udpReceiver.StartReceiving();
		}
		protected override void SpecificTerminate() {
			this.udpReceiver.OnDataReceived -= OnDataReceived;
			this.udpReceiver.StopReceiving();
			this.udpReceiver.Disconnect();
		}
		protected override void InitiateHoloComponent(){
			this._holoComponent = new HoloComponent(SourceType.UDP, SourceCommunicationType.Receiver, this.localPortOverride);
		}
		////////////////////////////////////////////////////////////////////////
		// - HoloSystemState
		private void InterpreteHoloState(string data){
			HoloSystemState serverState = EncodeUtilities.InterpreteHoloState(data);
			NetworkManager.instance.RequestUpdateServerState(serverState);
		}
		// - HoloTerminate
		private void InterpreteHoloTerminate(string data){
			NetworkManager.instance.RequestNetworkReset();
		}
	}
}