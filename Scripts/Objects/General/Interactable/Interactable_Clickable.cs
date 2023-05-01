// #define DEBUG
// #define DEBUG2
#undef DEBUG
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HoloFab {
    [RequireComponent(typeof(Collider))]
    public class Interactable_Clickable : MonoBehaviour {
		public UnityEvent OnTrigger;
        // Set initial state.
        void OnEnable(){
			InteractionManager.instance.OnGameObjectClick += CheckTrigger;
		}
        private void OnDisable() {
			if (InteractionManager.instance == null) return;
			InteractionManager.instance.OnGameObjectClick -= CheckTrigger;
        }
        public bool CheckTrigger(GameObject goHit){
			// Check if given object is the trigger and react.
			if (gameObject == goHit) {
				if (this.OnTrigger != null) { 
					#if DEBUG
        			Debug.Log("Interactable Clickable ["+gameObject.name+"]: Clicked on object: " + gameObject.name);
        			#endif
					this.OnTrigger.Invoke();
				}
				return true;
			} else
				return false;
		}
    }
}