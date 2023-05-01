//#define DEBUG
//#define DEBUG2
#define DEBUGWARNING
#undef DEBUG
#undef DEBUG2
//#undef DEBUGWARNING

using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Unity Component Interfacing UDP Receive class.
	public class UDPReceiveComponent : NetworkReceiverAgentComponent {
		[Header("Necessary Variables.")]
		[Tooltip("A port for UDP communication to listen on.")]
		public int localPortOverride = 8810;
        
		// Local Variables.
		protected override string sourceName { get { return "UDP Receive Component"; } }
		// protected override SourceType sourceType { get { return SourceType.UDP; } }
		private UDPReceive udpReceiver;
		protected override Dictionary<string, Interpreter> validInterpreters {
			get {
				return new Dictionary<string, Interpreter>(){
						   {"MESHSTREAMING", InterpreteMesh},
						   {"HOLOTAG", InterpreteLabel}
						   //{"HOLOBOTS". InterpreteHoloBots},
						   //{"CONTROLLER". InterpreteRobotController}
				};
			}
		}
		// // - IP Address received.
		// public static bool flagUICommunicationStarted = false;
		// public static bool flagEnvironmentSent = false;
        
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
		// - Mesh
		private void InterpreteMesh(string data){
			ObjectManager.instance.RequestNewData("MESHSTREAMING", data, this.sourceType);
		}
		// - Tag
		private void InterpreteLabel(string data){
			ObjectManager.instance.RequestNewData("HOLOTAG", data, this.sourceType);
		}
	}
}