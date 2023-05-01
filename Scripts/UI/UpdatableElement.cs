//#define DEBUG
#undef DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloFab {
    public abstract class UpdatableElement : MonoBehaviour{
		public delegate void OnItemUpdate();
        public OnItemUpdate onValueUpdated;

        protected void InformChange() { 
			if (this.onValueUpdated != null)
				this.onValueUpdated.Invoke();
        }
    }
}
