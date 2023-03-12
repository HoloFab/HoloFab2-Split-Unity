#define DEBUG
// #define DEBUG2
// #undef DEBUG
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloFab {
	// A structure to allow the object to snap to the scanned environment.
	public class Interactable_Placeable : MonoBehaviour {
		[Tooltip("If is placing on start.")]
		public bool flagPlacingOnStart = true;
		[Tooltip("If on placement orient as well.")]
		public bool flagOrient = false;
		[Tooltip("A distance to hover at.")]
		public float hoverDistance = 2f;
		[Tooltip("A distance to start snapping at.")]
		public float maxSnapDistance = 4f;
        
		// Internal variable to keep track of placement.
		private bool _flagPlacing;
		private bool _readyToPlace;

        public bool flagPlacing {
			get {
				return this._flagPlacing;
			}
			set {
				bool changed = (this.flagPlacing != value);
				this._flagPlacing = value;
				if (changed) { 
					#if DEBUG
        			Debug.Log("Interactable Placable ["+gameObject.name+"]: Changing state: " + this.flagPlacing);
        			#endif
					if (this._flagPlacing) {
						// Start Placing mode.
						//InteractionManager.instance.activePlaceable = this;
						InteractionManager.instance.OnTap += OnTrySnap;
						StartCoroutine(PlacingRoutine());
					} else { 
						//InteractionManager.instance.activePlaceable = null;
						InteractionManager.instance.OnTap -= OnTrySnap;
					}
				}
				UpdateAppearance();
            }
        }

		void OnEnable(){
			// Set initial state.
			this.flagPlacing = this.flagPlacingOnStart;
			//InteractionManager.instance.RegisterPlacable(this);
		}
		void OnDisable(){
			//InteractionManager.instance.UnregisterPlacable(this);
		}
        ////////////////////////////////////////////////////////////////////////
        public delegate void OnClickAction(); // Move to Meta Class
        public OnClickAction OnStartPlacing;
        public OnClickAction OnEndPlacing;
        private void UpdateAppearance(){
			// If events to trigger animations are set - call them.
			if ((this.flagPlacing) && (this.OnStartPlacing != null))
				this.OnStartPlacing.Invoke();
			else if ((!this.flagPlacing) && (this.OnEndPlacing != null))
                this.OnEndPlacing.Invoke();
		}
		////////////////////////////////////////////////////////////////////////
		private IEnumerator PlacingRoutine(){
			while (this.flagPlacing) {
				PlacePreview();
				yield return null;
			}
		}
		private void PlacePreview() {
            this._readyToPlace = false;
			float distance = this.hoverDistance;
			Vector3 normal = Vector3.up;
			//if ((InteractionManager.instance.flagHitGaze)
			//	&& (ObjectManager.instance.CheckValidPlacementObject(InteractionManager.instance.hitGaze.collider.gameObject))) {
			distance = Distance2Camera(InteractionManager.instance.hitGaze.point);
			if (distance < this.maxSnapDistance) {
				normal = InteractionManager.instance.hitGaze.normal;
				this._readyToPlace = true;
            }
			else { 
				distance = this.hoverDistance;
            }
			Place(distance, normal);
		}
		// Evaluate distance to the hit.
		private float Distance2Camera(Vector3 point){
			return (Camera.main.transform.position - point).magnitude;
		}
		// Position the object at a given point and orientation.
		private void Place(float distance, Vector3 normal){
			Vector3 position = Camera.main.transform.position 
				+ Camera.main.transform.forward * distance;
            
			transform.position = position;
			if (this.flagOrient)
				transform.localRotation = Quaternion.FromToRotation(transform.up, normal);
		}
		
		public void OnTrySnap(){
			// If clicked on mesh - try to snap if the object is currently placing.
			// NB! check the distance if the object is hovering (not on mesh) - ignore click.
			if (this.flagPlacing && this._readyToPlace) {
				this.flagPlacing = false;
			} else {
				DebugUtilities.UserMessage("Try to look at scanned mesh or come closer to it.");
			}
		}
	}
}