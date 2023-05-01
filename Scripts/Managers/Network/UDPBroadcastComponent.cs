//#define DEBUG
//#define DEBUG2
#define DEBUGWARNING
#undef DEBUG
#undef DEBUG2
//#undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;

using HoloFab;
using HoloFab.CustomData;

public class UDPBroadcastComponent : NetworkAgentComponent {
	// Settings:
	public int remotePort = 8801;
	public int expireTime = 3000;
	public string broadcasterName = "HelloWorld!";
	private string BroadcasterName {
		get { 
			#if UNITY_EDITOR
			return "Unity";
			#else
			return this.broadcasterName;
			#endif
		}
	}
	// Interanl Objects
	private byte[] requestData;
	private UDPBroadcast udpBroadcaster;
	protected override string sourceName { get { return "UDP Broadcasting Component"; } }
	// protected override SourceType sourceType { get { return SourceType.UDP; } }
    
	public TaskInterface sender;
    
	protected override void OnEnable() {
		base.OnEnable();
        
		this.requestData = Encoding.ASCII.GetBytes(this.BroadcasterName);
		this.udpBroadcaster = new UDPBroadcast(this, _port: this.remotePort, _ownerName: this.sourceName);
		this.udpBroadcaster.Connect();
        this.udpBroadcaster.StartSending();
        
		this.sender = new TaskInterface(Broadcast, this.expireTime, _taskName: this.sourceName+": Regular Broadcaster");
		this.sender.Start();
	}
	protected override void SpecificTerminate() {
		this.sender.Stop();
		this.udpBroadcaster.StopReceiving();
		this.udpBroadcaster.Disconnect();
	}
    
	protected override void InitiateHoloComponent(){
		this._holoComponent = new HoloComponent(SourceType.UDP, SourceCommunicationType.Sender, this.remotePort);
	}
	private void Broadcast(){
		#if DEBUG2
		DebugUtilities.UniversalDebug(this.sourceName, "Broadcasting a message: " + this.BroadCastMessage);
		#endif
        
		this.udpBroadcaster.QueueUpData(this.requestData);
		// if (!this.udpBroadcaster.flagSuccess) {
		// 	#if DEBUGWARNING
		// 	DebugUtilities.UniversalWarning(this.sourceName, "Couldn't broadcast the message.");
		// 	#endif
		// }
	}
}