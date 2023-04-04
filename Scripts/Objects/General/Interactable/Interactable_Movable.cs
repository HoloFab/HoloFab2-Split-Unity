#define DEBUG
#define DEBUG2
// #undef DEBUG
// #undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloFab {
	public class Interactable_Movable : Interactable {
		protected override bool flagInteracting { 
			get => (this.state != MovableInteractionType.Inactive);
		}
		public enum MovableInteractionType {Inactive, MoveXY, MoveZ, RotateZ}        
		private MovableInteractionType _state = MovableInteractionType.Inactive;
        public MovableInteractionType state {
			get { 
				return this._state; 
			}
            set {
				bool changed = (this._state != (MovableInteractionType)value);
				this._state = (MovableInteractionType)value;
				if (changed) {
					#if DEBUG
        			Debug.Log("Interactable Movable ["+gameObject.name+"]: Changing state: " + this._state);
        			#endif
					switch (this._state) {
						case MovableInteractionType.MoveXY:
                            this.InteracttionAction = MoveXY;
                            this.orientationPlane = new Plane(transform.up, transform.position);
                            break;
						case MovableInteractionType.MoveZ:
                            this.InteracttionAction = MoveZ;
                            this.orientationPlane = new Plane(Camera.main.transform.forward, transform.position);
                            break; 
						case MovableInteractionType.RotateZ:
                            this.InteracttionAction = RotateZ;
                            this.orientationPlane = new Plane(transform.up, transform.position);
                            break;
						default:
							this._state = MovableInteractionType.Inactive;
							return;
                    }
					if (this.flagInteracting) { 
						this.strartRelativeClickPoint = InteractionManager.instance.CheckPlane(this.orientationPlane);
						this.startPosition = transform.position;
						StartCoroutine(ActiveInteractionCoroutine());
						InteractionManager.instance.OnDragReleased += OnStopInteraction;
                    } else
                        InteractionManager.instance.OnDragReleased -= OnStopInteraction;

                    UpdateAppearance();
                }
            }
        }
		public void TriggerMoveXY() => this.state = MovableInteractionType.MoveXY;
		public void TriggerMoveZ() => this.state = MovableInteractionType.MoveZ;
		public void TriggerRotateZ() => this.state = MovableInteractionType.RotateZ;
		// Extract object orientation. Here so that if orienting by object can be done locally specifically here.
		private Plane orientationPlane;
		// A point of hit set from the interaction manager.
		private Vector3 startPosition, strartRelativeClickPoint;
		// Internal history of orientation for rotating.
		private Vector3 lastClickVector;

		void OnEnable(){
			//InteractionManager.instance.RegisterMovable(this);
		}
		void OnDisable(){
			//InteractionManager.instance.UnregisterMovable(this);
		}
		////////////////////////////////////////////////////////////////////////
		// Update position and rotation per frame.
		private void MoveXY(){
            Vector3 relativeClickPoint = InteractionManager.instance.CheckPlane(this.orientationPlane);
			Vector3 dragDifference = relativeClickPoint - this.strartRelativeClickPoint;
			
            float currentZ = transform.position.y;
			transform.position = this.startPosition + dragDifference;
			transform.position = new Vector3(transform.position.x, currentZ, transform.position.z);
		}
		private void MoveZ(){
            Vector3 relativeClickPoint = InteractionManager.instance.CheckPlane(this.orientationPlane);
            Vector3 dragDifference = relativeClickPoint - this.strartRelativeClickPoint;
            
			float newY = (this.startPosition + dragDifference).y;
			transform.position = new Vector3(transform.position.x, newY, transform.position.z);
		}
		// private void RotateZ(Vector3 currentDragOrientation){
		// 	Vector3 rt = Quaternion.AngleAxis(1, this.orientationPlane.normal) * this.lastDragOrientation; // a trick to check direction of rotation?
		// 	float a1 = Vector3.Angle(this.lastDragOrientation, currentDragOrientation);
		// 	float a2 = Vector3.Angle(rt, currentDragOrientation);
		// 	if (a2 > a1) a1 *= -1;
		// 	transform.RotateAroundLocal(this.orientationPlane.normal, Mathf.Deg2Rad * a1);
		// 	this.lastDragOrientation = currentDragOrientation;
		// }
		private void RotateZ(){
            Vector3 relativeClickPoint = InteractionManager.instance.CheckPlane(this.orientationPlane);
            Vector3 clickVector = relativeClickPoint - transform.position;

            // a trick to check direction of rotation?
            Vector3 controlVector = Quaternion.AngleAxis(1, this.orientationPlane.normal) * this.lastClickVector;
			float currentAngle = Vector3.Angle(this.lastClickVector, clickVector);
			float controlAngle = Vector3.Angle(controlVector, clickVector);
			if (controlAngle > currentAngle) currentAngle *= -1;
			this.lastClickVector = clickVector;

			transform.Rotate(this.orientationPlane.normal, currentAngle);// Mathf.Deg2Rad *
		}
		protected override void OnStopInteraction(){
			this._state = MovableInteractionType.Inactive;
		}
		////////////////////////////////////////////////////////////////////////
		public onInteractAction OnStartMoveXY;
		public onInteractAction OnStartMoveZ;
		public onInteractAction OnStartRotateZ;
        public onInteractAction OnEndInteractiion;
		// If events to trigger animations are set - call them.
		protected override void UpdateAppearance(){
			switch (this._state) {
				 case MovableInteractionType.MoveXY:
                    if (this.OnStartMoveXY != null)
                        this.OnStartMoveXY();
                    break;
				 case MovableInteractionType.MoveZ:
					 if (this.OnStartMoveZ != null)
						this.OnStartMoveZ();
                     break;
                case MovableInteractionType.RotateZ:
                    if (this.OnStartRotateZ != null)
                        this.OnStartRotateZ();
                    break;
                case MovableInteractionType.Inactive:
                    if (this.OnEndInteractiion != null)
                        this.OnEndInteractiion();
                    break;
                default:
					 break;
			}
		}
	}
}