// #define DEBUG
#define DEBUGWARNING
#undef DEBUG
// #undef DEBUGWARNING

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using HoloFab;
using HoloFab.CustomData;

namespace HoloFab {
	// Generatable UI Controlling manager.
	// TODO:
	// - Move canvases height offset (55) up.
	// - Check if generic version works. If so delete commented code.
	// - Later: Make into more generic list of types with corresponding variables.
	// - Later: Automatically find buttons and canvasses by tags (?)
	public class ParameterUIMenu : MonoBehaviour {
        
		// Necessary variables.
		[Header("Variables Set From Scene.")]
		[Tooltip("Buttons to control each type of generatable UI.")]
		public Button buttonBooleanAdder;
		[Tooltip("Buttons to control each type of generatable UI.")]
		public Button buttonCounterAdder;
		[Tooltip("Buttons to control each type of generatable UI.")]
		public Button buttonSliderAdder;
		[Tooltip("Button to delete UI items.")]
		public Button buttonDeleter;
		[Tooltip("Parent Canvases for each type of generatable UI.")]
		public Canvas canvasBooleanToggle, canvasCounter, canvasSlider;
        
		[Header("Variables Set From Prefabs.")]
		[Tooltip("Prefab Prefabs of each type of generatable UI.")]
		public GameObject goPrefabUIBooleanToggle;
		[Tooltip("Prefab Prefabs of each type of generatable UI.")]
		public GameObject goPrefabUICounter;
		[Tooltip("Prefab Prefabs of each type of generatable UI.")]
		public GameObject goPrefabUISlider;
        
		// Secondary Variables with presets.
		[Header("Preset Variables.")]
		//[Tooltip("Tags for generatable UI items.")]
		//public string tagUIItemBoolean = "Toggle";
		//[Tooltip("Tags for generatable UI items.")]
		//public string tagUIItemCounter = "Counter";
		//[Tooltip("Tags for generatable UI items.")]
		//public string tagUIItemSlider = "Slider";
		[Tooltip("Limiting amounts for each type of generatable UI.")]
		public int UILimitCount = 6;
        
		public int initialSize = 110;
		public int maximumSize = 625;
        
		[Header("Adjustable panel from scene")]
		[Tooltip("Adjustable UI panel")]
		public GameObject panel;
		private RectTransform rt;
		private float maxY;

		// Local variables.
		// Network variables.
		private NetworkAgentComponent sender => NetworkManager.instance[SourceType.UDP, SourceCommunicationType.Sender];
        // Stored message to avoid unnecessary traffic.
        private string lastMessage;

        private List<GameObject> goBooleans = new List<GameObject>(),
			goCounters = new List<GameObject>(),
			goSliders = new List<GameObject>();
        void Start() {
			// Instanses of panel variables
			this.rt = panel.GetComponent<RectTransform>();
			this.goBooleans = new List<GameObject>();
			this.goCounters = new List<GameObject>();
			this.goSliders = new List<GameObject>();
		}
		//////////////////////////////////////////////////////////////////////////
		// Generic UI adding function.
		private void TryAddUIItem(ref List<GameObject> trackedCategory, int limit,
				GameObject goPrefab, Canvas cParent, float height) {
			if (trackedCategory.Count < limit) {
				#if DEBUG
				Debug.Log("ParameterUIMenu: Adding new UI Element.");
				#endif
				//determinning the position in Y
				float poseY = trackedCategory.Count * height / limit;
                
				//Updating the size of panel
				if (poseY != 0) {
					if (maxY <= poseY) {
						rt.sizeDelta = new Vector2(rt.sizeDelta.x, poseY + initialSize);
						maxY = poseY;
					}
				}
                
				//Adding
				GameObject goUIItem = Instantiate(goPrefab, cParent.gameObject.transform);
				RectTransform rectTransform = goUIItem.GetComponent<RectTransform>();
				rectTransform.anchoredPosition = new Vector2(0, poseY);
				trackedCategory.Add(goUIItem);
                // Subscribe necessary events
                UpdatableElement element = goUIItem.GetComponentInChildren<UpdatableElement>();
				if (element != null) 
					element.onValueUpdated += OnUpdate;

                // System Update
                OnUpdate();
            }
		}
		// Add Boolean Toggle UI item.
		public void TryAddBooleanToggle() {
			TryAddUIItem(ref this.goBooleans, this.UILimitCount,
			             this.goPrefabUIBooleanToggle, this.canvasBooleanToggle,
			             this.maximumSize);
        }
		// Add Counter UI item.
		public void TryAddCounter() {
			TryAddUIItem(ref this.goCounters, this.UILimitCount,
			             this.goPrefabUICounter, this.canvasCounter,
			             this.maximumSize);
        }
		// Add Slider UI item.
		public void TryAddSlider() {
			TryAddUIItem(ref this.goSliders, this.UILimitCount,
			             this.goPrefabUISlider, this.canvasSlider,
			             this.maximumSize);
        }
		// Delete all user generated UIs.
		public void Clear() {            
			for (int i = this.goBooleans.Count-1; i >= 0; i--) DestroyImmediate(this.goBooleans[i]);
			for (int i = this.goCounters.Count - 1; i >= 0; i--) DestroyImmediate(this.goCounters[i]);
			for (int i = this.goSliders.Count - 1; i >= 0; i--) DestroyImmediate(this.goSliders[i]);
			this.goBooleans.Clear();
			this.goCounters.Clear();
			this.goSliders.Clear();
			Resources.UnloadUnusedAssets();
            
			// Inform UI Manager.
			OnUpdate();
            
			// Setting the Initial size of panel
			rt.sizeDelta = new Vector2(rt.sizeDelta.x, initialSize);
			maxY = initialSize;
		}
        
		//////////////////////////////////////////////////////////////////////////
		// React to a value change.
		public void OnUpdate() {
			#if DEBUG2
			Debug.Log("ParameterUIMenu: Updating UI values.\nFound items: "
				+ "booleans: " + goBooleans.Count 
				+ ", counters: " + goCounters.Count 
				+ ", sliders: " + goSliders.Count);
			#endif
            
			// Extract data.
			List<bool> bools = new List<bool>();
			List<int> ints = new List<int>();
			List<float> floats = new List<float>();
			foreach (GameObject goItem in goBooleans)
				bools.Add(goItem.GetComponent<BooleanToggle>().value);
			foreach (GameObject goItem in goCounters)
				ints.Add(goItem.GetComponent<Counter>().value);
			foreach (GameObject goItem in goSliders)
				floats.Add(goItem.GetComponent<FloatSlider>().value);
			UIData values = new UIData(bools, ints, floats);
            
			// Encode and if changed - send it.
			byte[] data = EncodeUtilities.EncodeData("UIDATA", values, out string currentMessage);
			if (this.lastMessage != currentMessage) {     // TODO: Technically not necessary now since we call directly from UI elements themselves.
				if (this.sender == null) {
					#if DEBUGWARNING
					Debug.Log("ParameterUIMenu: No sender Found.");
					#endif
					return;
				}
				this.lastMessage = currentMessage;
				#if DEBUG
				Debug.Log("ParameterUIMenu: values changed, sending: " + currentMessage);
				#endif
				((UDPSendComponent)this.sender).QueueUpData(data);
			}
		}
	}
}