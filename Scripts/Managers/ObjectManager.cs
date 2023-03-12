#define DEBUG
#define DEBUGWARNING
// #undef DEBUG
// #undef DEBUGWARNING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
#endif
#if UNITY_ANDROID
using System.Threading;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
// using GoogleARCore.Examples.Common;
// using GoogleARCore.Examples.HelloAR;
#endif

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Generatable Object manager.
	// TODO:
	// - Later: Move processors here?
	[RequireComponent(typeof(MeshProcessor))]
	[RequireComponent(typeof(LabelProcessor))]
	// [RequireComponent(typeof(RobotProcessor))]
	// [RequireComponent(typeof(Point3DProcessor))]
	public class ObjectManager : Type_Manager<ObjectManager> {
		// - CPlane object tag.
		private string tagCPlane = "CPlane";
		//private string layerScanMesh = "Spatial Awareness";
		// - Local reference of CPlane object
		public GameObject cPlane;
		// - Meshes of the environment
		public List<GameObject> scannedEnvironment;
		// Keep track of the scanned grid status.
		[HideInInspector]
		public bool flagGridVisible = true;
		private MeshProcessor meshProcessor;
		private LabelProcessor labelProcessor;
		// private RobotProcessor robotProcessor;
		// private Point3DProcessor point3DProcessor;
        
		private Queue<ReceivedData> incomingQueue = new Queue<ReceivedData>();
		private struct ReceivedData {
			public string header;
			public string data;
			public SourceType sourceType;
			public ReceivedData(string _header, string _data, SourceType _sourceType){
				this.header = _header;
				this.data = _data;
				this.sourceType = _sourceType;
			}
		}
        
		// Local Variables.
		private string sourceName = "Object Manager";
		#if UNITY_ANDROID
		private ARPlaneManager planeManager;
        private ARPointCloudManager pointCloudManager;
		#endif

		protected override void Awake(){
			base.Awake();
			this.meshProcessor = GetComponent<MeshProcessor>();
			this.labelProcessor = GetComponent<LabelProcessor>();
			// this.robotProcessor = GetComponent<RobotProcessor>();
			// this.point3DProcessor = GetComponent<Point3DProcessor>();
			#if UNITY_ANDROID
			this.planeManager = FindObjectOfType<ARPlaneManager>();
			this.pointCloudManager = FindObjectOfType<ARPointCloudManager>();
			#endif

        }
		void Start(){
			StartCoroutine(Introduction());
		}
		private IEnumerator Introduction() { 
			DebugUtilities.UserMessage("Hollo World . . .");
			yield return new WaitForSeconds(1.500f);
			DebugUtilities.UserMessage("Welcome to Holofab!");
			yield return new WaitForSeconds(1.500f);
			DebugUtilities.UserMessage("Your IP is:\n" + NetworkUtilities.LocalIPAddress());
			yield return new WaitForSeconds(3.500f);
			DebugUtilities.UserMessage("Place your CPlane by tapping on scanned mesh.");
			yield return null;
		}
        
		void Update(){
			lock (this.incomingQueue) {
				while (this.incomingQueue.Count > 0) {
					ReceivedData latestItem = this.incomingQueue.Dequeue();
					ProcessNewData(latestItem.header, latestItem.data, latestItem.sourceType);
				}
			}
		}
		////////////////////////////////////////////////////////////////////////
		// If c plane is not found - hint user and return false.
		public bool CheckCPlane(){
			if (this.cPlane == null) {
				this.cPlane = GameObject.FindGameObjectWithTag(this.tagCPlane);
				if (this.cPlane == null) {
					DebugUtilities.UserMessage("Place your CPlane by tapping on scanned mesh.");
					return false;
				}
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "CPlane: " + this.cPlane);
				#endif
			}
			// #if UNITY_ANDROID
			// HoloFabARController.cPlaneInstance = this.cPlane;
			// #endif
			return true;
		}
		////////////////////////////////////////////////////////////////////////
		// Functions to interprete and react to determined type of messages: // TODO: Join with UDP interpreters?
		public void RequestNewData(string _header, string _data, SourceType _sourceType){
			// Because networking is now fully on separate task
			// and Unity doesn't work with geometrty on any thread except the main.
			lock (this.incomingQueue) {
				this.incomingQueue.Enqueue(new ReceivedData(_header, _data, _sourceType));
			}
		}
		private void ProcessNewData(string _header, string _data, SourceType _sourceType){
			switch (_header) {
			 case "MESHSTREAMING":
				 InterpreteMesh(_data, _sourceType);
				 break;
			 case "HOLOTAG":
				 InterpreteLabel(_data);
				 break;
			}
		}
		// - Mesh
		private void InterpreteMesh(string data, SourceType meshSourceType){
			this.meshProcessor.ProcessMesh(EncodeUtilities.InterpreteMesh(data), meshSourceType);
		}
		// - Tag
		private void InterpreteLabel(string data){
			this.labelProcessor.ProcessTag(EncodeUtilities.InterpreteLabel(data));
		}
		// // - HoloBots
		// private void InterpreteHoloBots(string data){
		// 	this.robotProcessor.ProcessRobot(EncodeUtilities.InterpreteHoloBots(data));
		// }
		// // - RobotControllers
		// private void InterpreteRobotController(string data){
		// 	List<RobotControllerData> controllersData = EncodeUtilities.InterpreteRobotController(data);
		//
		// 	RobotProcessor processor = GetComponent<RobotProcessor>();
		// 	foreach (RobotControllerData controllerData in controllersData)
		// 		if(this.robotProcessor.robotsInstantiated.ContainsKey(controllerData.robotID))
		// 			this.robotProcessor.robotsInstantiated[controllerData.robotID].GetComponentInChildren<RobotController>().ProcessRobotController(controllerData);
		// }
		////////////////////////////////////////////////////////////////////////
		// Find environment meshes
		public void FindMeshes(){
			this.scannedEnvironment = new List<GameObject>();
			#if WINDOWS_UWP
			// Microsoft Windows MRTK
			// Cast the Spatial Awareness system to IMixedRealityDataProviderAccess to get an Observer
			var access = CoreServices.SpatialAwarenessSystem as IMixedRealityDataProviderAccess;
			// Get the first Mesh Observer available, generally we have only one registered
			var observers = access.GetDataProviders<IMixedRealitySpatialAwarenessMeshObserver>();
			// Loop through all known Meshes
			foreach (var observer in observers) {
				foreach (SpatialAwarenessMeshObject meshObject in observer.Meshes.Values) {
					this.scannedEnvironment.Add(meshObject.Renderer);
				}
			}
			#elif UNITY_ANDROID
			// Android ARCore
			foreach (ARPlane plane in this.planeManager.trackables)
                this.scannedEnvironment.Add(plane.gameObject);
			foreach (ARPointCloud pointCloud in this.pointCloudManager.trackables)
				this.scannedEnvironment.Add(pointCloud.gameObject);
            #elif UNITY_EDITOR
			GameObject[] goItems = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			//will return an array of all GameObjects in the scene
			foreach(GameObject goItem in goItems) {
				if(goItem.layer == LayerMask.NameToLayer(this.layerScanMesh)) {
					this.scannedEnvironment.Add(goItem);
				}
			}
			#endif
			#if DEBUG
            DebugUtilities.UniversalDebug(this.sourceName, "Meshes found: " + this.scannedEnvironment.Count);
			#endif
		}
		// Toggle all meshes.
		public void ToggleEnvironmentMeshes(){
			FindMeshes();
			this.flagGridVisible = !this.flagGridVisible;
			foreach (GameObject environmentObjects in this.scannedEnvironment) {
				Renderer[] renderers = environmentObjects.GetComponents<Renderer>();
				foreach (Renderer renderer in renderers) {
					renderer.enabled = this.flagGridVisible;
				}
			}
			
			#if WINDOWS_UWP
			// Microsoft Windows MRTK
			// Toggle Mesh Observation from all Observers
			if (ObjectManager.instance.flagGridVisible)
				CoreServices.SpatialAwarenessSystem.ResumeObservers();
			else
				CoreServices.SpatialAwarenessSystem.SuspendObservers();
			#else
			this.planeManager.enabled = this.flagGridVisible;
			this.pointCloudManager.enabled = this.flagGridVisible;
			#endif
		}
        
		// Collect environment meshes
		public List<byte[]> EncodeEnvironmentMesh(out string currentMessage){
			currentMessage = string.Empty;
			List<byte[]> data = new List<byte[]>();
			FindMeshes();
			if (this.scannedEnvironment.Count > 0) {
				// Combine meshes
				CombineInstance[] combineStructure = new CombineInstance[this.scannedEnvironment.Count];
				int i = 0;
				foreach (GameObject goItem in this.scannedEnvironment) {
					MeshFilter meshFilter = goItem.GetComponent<MeshFilter>();
					combineStructure[i].mesh = meshFilter.sharedMesh;
					combineStructure[i].transform = goItem.transform.localToWorldMatrix;
					i++;
				}
				Mesh mesh = new Mesh();
				mesh.CombineMeshes(combineStructure);
                
				// Encode mesh
				MeshData meshData = MeshUtilities.EncodeMesh(mesh);
				byte[] localData = EncodeUtilities.EncodeData("ENVIRONMENT", meshData, out string currentLocalMessage);
				data.Add(localData);
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "Mesh Encoding: " + currentLocalMessage);
				#endif
				currentMessage = currentLocalMessage;
                
				// // Encode meshes separately
				// // {
				// // 	MeshRenderer meshRenderer = this.scannedEnvironment[0];
				// foreach (MeshRenderer meshRenderer in this.scannedEnvironment) {
				// 	MeshFilter meshFilter = meshRenderer.gameObject.GetComponent<MeshFilter>();
				// 	MeshData meshData = MeshUtilities.EncodeMesh(meshFilter.sharedMesh);
				// 	byte[] localData = EncodeUtilities.EncodeData("ENVIRONMENT", meshData, out string currentLocalMessage);
				// 	#if DEBUG
				// 	DebugUtilities.UniversalDebug(this.sourceName, "Mesh Encoding: " + currentLocalMessage);
				// 	#endif
				// 	data.Add(localData);
				// 	currentMessage += currentLocalMessage;
				// }
			} else {
				#if DEBUG
				DebugUtilities.UniversalDebug(this.sourceName, "No meshes found.");
				#endif
			}
            
			return data;
		}
	}
}