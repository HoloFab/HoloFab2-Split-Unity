﻿// #define DEBUG
// #define DEBUG2
#undef DEBUG
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloFab { 
    public abstract class Interactable : MonoBehaviour {
        protected abstract bool flagInteracting{
            get;
        }
        ////////////////////////////////////////////////////////////////////////
        protected delegate void interacttionAction();
        protected interacttionAction InteracttionAction;
		protected IEnumerator ActiveInteractionCoroutine(){
			while (this.flagInteracting) {
				if (this.InteracttionAction != null) 
                    this.InteracttionAction.Invoke();
                if (this.OnInteracting != null)
                    this.OnInteracting.Invoke();

                yield return null;
			}
        }
        protected abstract void OnStopInteraction();
        ////////////////////////////////////////////////////////////////////////
        public delegate void onInteractAction();
        public onInteractAction OnInteracting;
        protected abstract void UpdateEvents();
    }
}
