//#define DEBUG
//#define DEBUG2
#define DEBUGWARNING
#undef DEBUG
#undef DEBUG2
//#undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Inititate, keep track and manage sending data of Marked Points.
	public class Point3DProcessor : MonoBehaviour {
		// An example of 3D point.
		public GameObject goPoint3DExample;
        
		// History of created objects.
		private List<Point3DController> markedPoints;
        
		// Network variables.
        private NetworkAgentComponent sender => NetworkManager.instance[SourceType.UDP, SourceCommunicationType.Sender];
        // Stored message to avoid unnecessary traffic.
        private string lastMessage;

        void OnEnable(){
			this.markedPoints = new List<Point3DController>();
		}
		// Add a point.
		public void AddPoint(){
			if (this.goPoint3DExample != null) {
				GameObject goItem = Instantiate(this.goPoint3DExample,
				                                Camera.main.transform.position + Camera.main.transform.forward,
				                                ObjectManager.instance.cPlane.transform.rotation,
				                                ObjectManager.instance.cPlane.transform);
				Point3DController controller = goItem.GetComponent<Point3DController>();
                this.markedPoints.Add(controller);
                // Subscribe necessary events
                if (controller != null)
                    controller.onValueUpdated += OnUpdate;
				// System Update
                OnUpdate();
			}
		}
		// Delete all previously created points.
		public void Clear(){
			for (int i=this.markedPoints.Count-1; i >= 0; i--)
				GameObject.DestroyImmediate(this.markedPoints[i].gameObject);
			this.markedPoints.Clear();
		}

		public void OnUpdate() { 
			#if DEBUG2
			Debug.Log("Point3D Processor: Updating Marked Point values.\nFound Objects: " + this.markedPoints.Count);
			#endif
			
			// Extract data.
			List<float[]> points = new List<float[]>();
            List<float[]> normals = new List<float[]>();
            foreach (Point3DController item in this.markedPoints) {
				points.Add(EncodeUtilities.EncodeLocation(item.RelativePosition));
                normals.Add(EncodeUtilities.EncodeLocation(item.Up));
            }
			MarkedPointData values = new MarkedPointData(points, normals);
            
			// Encode and if changed - send it.
			byte[] data = EncodeUtilities.EncodeData("MPDATA", values, out string currentMessage);
			if (this.lastMessage != currentMessage) {
				if (this.sender == null) {
					#if DEBUGWARNING
					Debug.Log("Point3D Processor: No sender Found.");
					#endif
					return;
				}
				this.lastMessage = currentMessage;
				#if DEBUG
				Debug.Log("Point3D Processor: values changed, sending: " + currentMessage);
				#endif
				((UDPSendComponent)this.sender).QueueUpData(data);
			}
		}
    }
}