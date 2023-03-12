#define DEBUG
// #define DEBUG2
#define DEBUGWARNING
// #undef DEBUG
#undef DEBUG2
// #undef DEBUGWARNING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;
using System.Linq;

namespace HoloFab {
	[RequireComponent(typeof(UDPBroadcastComponent))]
	[RequireComponent(typeof(UDPSystemSyncComponent))]
	[RequireComponent(typeof(UDPClientAcknowledgerComponent))]
	public class NetworkManager : Type_Manager<NetworkManager> {
		private string sourceName = "Network Manager";
		[SerializeField]
		private HoloSystemState _holoState = new HoloSystemState();
		public HoloSystemState holoState {
			get {
				return this._holoState;
			}
		}
		// Received IP address of the computer.
		public string remoteIP {
			get {
				if (this._holoState != null)
					return this._holoState.serverIP;
				return null;// NetworkUtilities.LocalIPAddress();
			}
		}
		private bool _connected;
		public bool connected {
			get {
				return this._connected;
			}
			set {
				this._connected = value;
			}
		}
		private bool requestAgentUpdate = false;
		private bool requestIPUpdate = false;
		private bool requestNetworkReset = false;
		private Queue<HoloComponent> newComponents = new Queue<HoloComponent>();
        
		//[SerializeField]
		private Dictionary<int, NetworkAgentComponent> registeredAgents = new Dictionary<int, NetworkAgentComponent>();
        
		private UDPClientAcknowledgerComponent acknowledger;
        
		public void RequestUpdateServerState(HoloSystemState receivedState){
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Remote IP: " + receivedState.serverIP);
			#endif
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "New State: " + receivedState.ToString() + ". Old State: " + this._holoState.ToString());
			#endif
			// TODO: Add ip validity check
			if ((this._holoState.serverIP != receivedState.serverIP)) {
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Server IP mismatch");
				#endif
				this._holoState = receivedState;
				this.requestNetworkReset = true;
				this.requestIPUpdate = true;
			}
			foreach(HoloComponent component in receivedState.holoComponents)
				if (!this._holoState.ContainsID(component.id) 
					&& !this.newComponents.Any(item => item.id == component.id)
					&& !this.registeredAgents.ContainsKey(component.id)) {
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "New Agent Requested " + component.id);
					#endif
					this.requestAgentUpdate = true;
					lock (this.newComponents) {
						this.newComponents.Enqueue(component);
					}
				}
		}
		public void RequestNetworkReset(){
            this._holoState.Clear();
            
			this.requestNetworkReset = true;
		}
        
		public void RegisterNetworkAgnetComponent(int componentID, NetworkAgentComponent networkAgentComponent){
			if (!this.registeredAgents.ContainsKey(componentID))
				this.registeredAgents.Add(componentID, networkAgentComponent);
		}
		public void UnRegisterNetworkAgnetComponent(int componentID, NetworkAgentComponent networkAgentComponent){
			if (this.registeredAgents.ContainsKey(componentID))
				this.registeredAgents.Remove(componentID);
		}
		///////////////////////////////////////////////////////////////////////
        
		protected override void Awake(){
			base.Awake();
			this.acknowledger = GetComponent<UDPClientAcknowledgerComponent>();
		}
		private void Update(){
			if (this.requestNetworkReset)
				ResetNetwork();
			if (this.requestIPUpdate)
				UpdateIP();
            if (NetworkManager.instance.connected && this.requestAgentUpdate)
                UpdateAgents();
        }
        
		private void UpdateAgents(){
			this.requestAgentUpdate = false;
			HoloComponent component = null;
			lock (this.newComponents) {
				if (this.newComponents.Count > 0) {
					component = this.newComponents.Dequeue();
				}
			}
			if (component == null) return;
			this._holoState.holoComponents.Add(component);
			switch (component.sourceType) {
				case SourceType.UDP:
					switch (component.communicationType) {
					case SourceCommunicationType.Sender:
					{
						UDPReceiveComponent networkAgentComponent = gameObject.AddComponent<UDPReceiveComponent>();
					}
					break;
					case SourceCommunicationType.Receiver:
					{
						UDPSendComponent networkAgentComponent = gameObject.AddComponent<UDPSendComponent>();
					}
					break;
					}
					break;
				case SourceType.TCP:
					switch (component.communicationType) {
					case SourceCommunicationType.Sender:
					case SourceCommunicationType.Receiver:
					case SourceCommunicationType.SenderReceiver:
						TCPAgentComponent networkAgentComponent = gameObject.AddComponent<TCPAgentComponent>();
						networkAgentComponent.localPortOverride = component.port;
						break;
					}
					break;
			}
			this.acknowledger.Acknowledge();
            
			// // TODO: Add ip integrity check
			//
			// // - UDP
			// // TODO: Should not be stored in udp sender.
			// UDPSendComponent udpSender = gameObject.GetComponent<UDPSendComponent>();
			// if (udpSender != null) {
			// 	udpSender.remoteIP = remoteIP;
			// 	UDPReceiveComponent.flagUICommunicationStarted = true;
			// 	// Inform UI Manager.
			// 	ParameterUIMenu.instance.OnValueChanged();
			// }
			//
			// // - TCP
			// TCPSendComponent tcpSender = gameObject.GetComponent<TCPSendComponent>();
			// #if DEBUG
			// DebugUtilities.UniversalDebug(this.sourceName, "Tcp sender found. Sending mesh to: " + remoteIP);
			// #endif
			// if (tcpSender != null) {
			// 	tcpSender.UpdateIP(remoteIP);
			// 	// byte[] environmentData = ObjectManager.instance.EnvironmentMesh();
			// 	// // Echo Environment Mesh
			// 	// if (environmentData != null) {
			// 	// 	tcpSender.remoteIP = remoteIP;
			// 	// 	tcpSender.SendMesh(environmentData);
			// 	// 	UDPReceiveComponent.flagEnvironmentSent = true;
			// 	// }
			// }
		}
		private void UpdateIP(){
			this.requestIPUpdate = false;
			foreach(NetworkAgentComponent agent in this.registeredAgents.Values)
				agent.OnUpdateIP();
			NetworkManager.instance.connected = true;
		}
		private void ResetNetwork(){
			this.requestNetworkReset = false;
			{
				UDPSendComponent[] components = GetComponents<UDPSendComponent>();
				for(int i = components.Length-1; i >= 0; i--) {
					components[i].Terminate();
					DestroyImmediate(components[i]);
				}
			}
			{
				UDPReceiveComponent[] components = GetComponents<UDPReceiveComponent>();
				for(int i = components.Length-1; i >= 0; i--) {
					components[i].Terminate();
					DestroyImmediate(components[i]);
				}
			}
			{
				TCPAgentComponent[] components = GetComponents<TCPAgentComponent>();
				for(int i = components.Length-1; i >= 0; i--) {
					components[i].Terminate();
					DestroyImmediate(components[i]);
				}
			}
			NetworkManager.instance.connected = false;
		}
	}
	public abstract class NetworkAgentComponent : MonoBehaviour {
		// Received IP address of the computer.
		protected string remoteIP => NetworkManager.instance.remoteIP;
		protected abstract string sourceName {get;}
		protected HoloComponent _holoComponent;
		public HoloComponent holoComponent {
			get {
				if (this._holoComponent == null)
					InitiateHoloComponent();
				return this._holoComponent;
			}
		}
		protected SourceType sourceType {
			get {
				if (this.holoComponent != null)
					return this.holoComponent.sourceType;
				return default(SourceType);
			}
		}
        
		protected virtual void OnEnable(){
			InitiateHoloComponent();
			NetworkManager.instance.RegisterNetworkAgnetComponent(this.holoComponent.id, this);
		}
		protected void OnDisable(){
			Terminate();
		}
		protected void OnApplicationQuit(){
			Terminate();
		}
        
		public virtual void OnUpdateIP(){}
		protected abstract void InitiateHoloComponent();
        
		public void Terminate(){
			DefaultTerminate();
			SpecificTerminate();
		}
		protected void DefaultTerminate(){
			NetworkManager.instance.UnRegisterNetworkAgnetComponent(this.holoComponent.id, this);
		}
		protected abstract void SpecificTerminate();
	}
	public delegate void Interpreter(string content);
	public abstract class NetworkReceiverAgentComponent : NetworkAgentComponent {
		protected abstract Dictionary<string, Interpreter> validInterpreters {
			get;
		}
		protected string lastMessageReceive;
		protected void OnDataReceived(object source, DataReceivedArgs data){
			string message = data.data;
			#if DEBUG
			DebugUtilities.UniversalDebug(this.sourceName, "Parsing input . . .");
			#endif
			if (!string.IsNullOrEmpty(message)) {
				message = EncodeUtilities.StripSplitter(message);
				if (this.lastMessageReceive != message) {
					this.lastMessageReceive = message;
					#if DEBUG
					DebugUtilities.UniversalDebug(this.sourceName, "New message found: " + message);
					#endif
					string[] messageComponents = message.Split(new string[] {EncodeUtilities.headerSplitter}, 2, StringSplitOptions.RemoveEmptyEntries);
					if (messageComponents.Length > 1) {
						string header = messageComponents[0], content = messageComponents[1];
						#if DEBUG
						DebugUtilities.UniversalDebug(this.sourceName, "Header: " + header + ", content: " + content);
						#endif
						foreach(KeyValuePair<string, Interpreter> interpreter in this.validInterpreters) {
							if (header == interpreter.Key) {
								interpreter.Value.Invoke(content);
								return;
							}
						}
						#if DEBUGWARNING
						DebugUtilities.UniversalWarning(this.sourceName, "Header Not Recognized");
						#endif
					} else {
						#if DEBUGWARNING
						DebugUtilities.UniversalWarning(this.sourceName, "Improper message");
						#endif
					}
				}
			}
			return;
		}
	}
}