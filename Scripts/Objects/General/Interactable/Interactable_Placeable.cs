//#define DEBUG
//#define DEBUG2
#undef DEBUG
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloFab {
	// A structure to allow the object to snap to the scanned environment.
	public class Interactable_Placeable : Interactable {
		[Tooltip("If is placing on start.")]
		public bool flagPlacingOnStart = true;
		[Tooltip("If on placement orient as well.")]
		public bool flagOrient = false;
		[Tooltip("A distance to hover at.")]
		public float hoverDistance = 2f;
		[Tooltip("A distance to start snapping at.")]
		public float maxSnapDistance = 4f;
        
		protected override bool flagInteracting { 
			get => this.state;
        }
        // Internal variable to keep track of placement.
        private bool _state = false;
        private bool _readyToPlace = false;
        public bool state {
			get { 
				return this._state;
			}
			set {
				bool changed = (this._state != value);
				this._state = value;
				if (changed) { 
					#if DEBUG
        			Debug.Log("Interactable Placable ["+gameObject.name+"]: Changing state: " + this._state);
        			#endif
					if (this._state) {
						// Start Placing mode.
						//InteractionManager.instance.activePlaceable = this;
						InteractionManager.instance.OnTap += OnStopInteraction;
						this.InteracttionAction = PlacePreview;
                        StartCoroutine(ActiveInteractionCoroutine());
					} else { 
						//InteractionManager.instance.activePlaceable = null;
						InteractionManager.instance.OnTap -= OnStopInteraction;
					}
				}
				UpdateEvents();
            }
        }

		void OnEnable(){
			// Set initial state.
			this.state = this.flagPlacingOnStart;
			//InteractionManager.instance.RegisterPlacable(this);
		}
		void OnDisable(){
			//if (InteractionManager.instance == null) return;
			//InteractionManager.instance.UnregisterPlacable(this);
		}
		////////////////////////////////////////////////////////////////////////
		private void PlacePreview() {
            this._readyToPlace = false;
			float distance = this.hoverDistance;
			Vector3 normal = Vector3.up;
			if (InteractionManager.instance.flagHitGaze) { 
				//	&& (ObjectManager.instance.CheckValidPlacementObject(InteractionManager.instance.hitGaze.collider.gameObject))) {
				distance = Distance2Camera(InteractionManager.instance.hitGaze.point);
				if (distance < this.maxSnapDistance) {
					normal = InteractionManager.instance.hitGaze.normal;
					this._readyToPlace = true;
				}
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
		protected override void OnStopInteraction(){
			// If clicked on mesh - try to snap if the object is currently placing.
			// NB! check the distance if the object is hovering (not on mesh) - ignore click.
			if (this._state && this._readyToPlace) {
				this.state = false;
			} else {
				DebugUtilities.UserMessage("Try to look at scanned mesh or come closer to it.");
			}
		}
        ////////////////////////////////////////////////////////////////////////
        public onInteractAction OnStartPlacing;
        public onInteractAction OnEndPlacing;
        protected override void UpdateEvents(){
			// If events to trigger animations are set - call them.
			if ((this._state) && (this.OnStartPlacing != null))
				this.OnStartPlacing.Invoke();
			else if ((!this._state) && (this.OnEndPlacing != null))
                this.OnEndPlacing.Invoke();
		}
	}
}