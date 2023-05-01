//#define DEBUG
//#define DEBUG2
#define DEBUGWARNING
#undef DEBUG
#undef DEBUG2
//#undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

namespace HoloFab {
	// Manage wether is currently interactable and manage animations and data label.
	[RequireComponent(typeof(Interactable_Movable))]
    [RequireComponent(typeof(Interactable_Placeable))]
    public class Point3DController : UpdatableElement {
		private bool flagOpen;

		// Label variables
		private TextMeshProUGUI textHolder;
        private Animator[] animators;

        public Vector3 RelativePosition {
			get {
				return ObjectManager.instance.cPlane.transform.InverseTransformPoint(transform.position);
            }
		}
		public Vector3 Up {
			get {
				return ObjectManager.instance.cPlane.transform.InverseTransformDirection(transform.up);
            }
		}

		private string sourceName = "Point 3D Controller";

		void Start() {
			this.flagOpen = false;

			this.animators = GetComponentsInChildren<Animator>();
			this.textHolder = GetComponentInChildren<TextMeshProUGUI>();

            Interactable_Movable movable = gameObject.GetComponent<Interactable_Movable>();
            if (movable != null)
            {
                movable.OnInteracting += UpdatePointLabel;
                movable.OnEndInteractiion += UpdatePointLabel;
                movable.OnEndInteractiion += InformChange;
            }
            Interactable_Placeable placeable = gameObject.GetComponent<Interactable_Placeable>();
            if (placeable != null)
            {
                // placeable.flagPlacingOnStart = true;
                placeable.OnStartPlacing += ToggleState;
                placeable.OnInteracting += UpdatePointLabel;
                placeable.OnEndPlacing += ToggleState;
                placeable.OnEndPlacing += UpdatePointLabel;
                placeable.OnEndPlacing += InformChange;
            }

            UpdateAnimationState();
			UpdatePointLabel();
        }
		// Accessible way to triger animation change.
		public void ToggleState(){
			this.flagOpen = !this.flagOpen;
			UpdateAnimationState();
			#if DEBUG
			DebugUtilities.UniversalDebug(sourceName, "Toggling state: New State: " + this.flagOpen);
			#endif
		}
		////////////////////////////////////////////////////////////////////////
		// A function to Update the point Label.
		private void UpdatePointLabel(){
			if (!ObjectManager.instance.CheckCPlane()) return;
			this.textHolder.text = "X: " + (this.RelativePosition.x*1000f).ToString("F2") + "\n" +
			                       "Y: " + (this.RelativePosition.z*1000f).ToString("F2") + "\n" +
			                       "Z: " + (this.RelativePosition.y*1000f).ToString("F2");
		}
		// General way to update animations.
		// TODO: Make one animation for all of them together
		private void UpdateAnimationState(){
			foreach (Animator animator in this.animators)
				animator.SetBool("Expand", this.flagOpen);
		}
	}
}
