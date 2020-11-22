//#define DEBUG2
#define DEBUGWARNING
#undef DEBUG2
// #undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;

using HoloFab;
using HoloFab.CustomData;

public class UDPBroadcastComponent : MonoBehaviour {
	// Settings:
	public int remotePort = 8888;
	public float expireTime = 1f;
	public string broadcastMessage = "HelloWorld!";
	// Interanl Objects
	private byte[] requestData;
	private UDPSend udpBroadcaster;
	private IEnumerator broadcastingRoutine;
	private string sourceName = "UDP Broadcasting Component";
    
	private ThreadInterface broadcastingThread;
    
	// Start is called before the first frame update
	void OnEnable() {
		this.requestData = Encoding.ASCII.GetBytes(this.broadcastMessage);
		this.udpBroadcaster = new UDPSend(string.Empty, this.remotePort);
		// this.broadcastingRoutine = BroadcastingRoutine();
		// StartCoroutine(this.broadcastingRoutine);
		this.broadcastingThread = new ThreadInterface(Broadcast,
		                                              Convert.ToInt32(this.expireTime)*1000);
		this.broadcastingThread.Start();
	}
	void OnDisable() {
		// StopCoroutine(this.broadcastingRoutine);
		this.broadcastingThread.Stop();
	}
	// private IEnumerator BroadcastingRoutine(){
	// 	while (true) {
	// 		Broadcast();
	// 		yield return new WaitForSeconds(this.expireTime);
	// 	}
	// }
	private void Broadcast(){
		#if DEBUG2
		DebugUtilities.UniversalDebug(this.sourceName, "Broadcasting a message: " + this.broadcastMessage);
		#endif
		this.udpBroadcaster.Broadcast(this.requestData);
		if (!this.udpBroadcaster.flagSuccess) {
			#if DEBUGWARNING
			DebugUtilities.UniversalWarning(this.sourceName, "Couldn't broadcast the message.");
			#endif
		}
	}
}