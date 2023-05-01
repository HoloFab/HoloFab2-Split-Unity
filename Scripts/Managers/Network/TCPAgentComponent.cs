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
	// Unity Component Interfacing TCP Agent class.
	public partial class TCPAgentComponent : NetworkReceiverAgentComponent {
		// Local Variables.
		protected override string sourceName { get { return "TCP Agent Component"; } }
		// protected override SourceType sourceType { get { return SourceType.TCP; } }
		[Header("Necessary Variables.")]
		private TCPAgent tcpAgent = null;
        
		// Unity Functions.
		protected override void OnEnable() {
			base.OnEnable();
            
			if (this.tcpAgent != null)
				this.tcpAgent.Disconnect();
			this.tcpAgent = new TCPAgent(this, this.remoteIP, this.localPortOverride, _ownerName: this.sourceName, _agentType: TCPAgent.AgentType.Client);
			this.tcpAgent.Connect();
            
			Receive_Enable();
			Send_Enable();
		}
		protected override void SpecificTerminate() {
			Receive_SpecificTerminate();
			Send_SpecificTerminate();
			this.tcpAgent.Disconnect();
		}
		protected override void InitiateHoloComponent(){
			this._holoComponent = new HoloComponent(SourceType.TCP, SourceCommunicationType.SenderReceiver, this.localPortOverride);
		}
	}
}