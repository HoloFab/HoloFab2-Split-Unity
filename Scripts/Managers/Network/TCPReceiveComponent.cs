#define DEBUG
// #define DEBUG2
#define DEBUGWARNING
// #undef DEBUG
#undef DEBUG2
// #undef DEBUGWARNING

using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

/// <summary>
/// Receives Data from MeshStreaming and Positioner Component
/// </summary>


namespace HoloFab {
	// Receive part of Unity Component Interfacing TCP Agent class.
	public partial class TCPAgentComponent {
		[Tooltip("A port for TCP communication to listen on.")]
		public int localPortOverride = 12121;
        
		// Local Variables.
		protected override Dictionary<string, Interpreter> validInterpreters {
			get {
				return new Dictionary<string, Interpreter>(){
						   {"MESHSTREAMING", InterpreteMesh}
						   //{"HOLOBOTS". InterpreteHoloBots},
						   //{"CONTROLLER". InterpreteRobotController}
				};
			}
		}
        
		// Unity Functions.
		private void Receive_Enable() {
			this.tcpAgent.OnDataReceived += OnDataReceived;
			this.tcpAgent.StartReceiving();
		}
		private void Receive_SpecificTerminate() {
			this.tcpAgent.OnDataReceived -= OnDataReceived;
			this.tcpAgent.StopReceiving();
		}
		// void Update() {
		// 	// if (!this.tcpAgent.flagConnectionFound) {
		// 	// 	#if DEBUGWARNING && DEBUG2
		// 	// 	DebugUtilities.UniversalWarning(this.sourceName, "Connection not Found.");
		// 	// 	#endif
		// 	// 	this.tcpAgent.Connect();
		// 	// 	if (!this.tcpAgent.flagConnectionFound) return;
		// 	// }
		// 	if (this.tcpAgent.dataMessages.Count > 0) {}
		// }
		////////////////////////////////////////////////////////////////////////
		// - Mesh
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