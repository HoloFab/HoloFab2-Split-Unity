#define DEBUG
// #define DEBUG2
// #undef DEBUG
#undef DEBUG2

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
#endif
using UnityEngine.InputSystem;

using HoloFab;
using HoloFab.CustomData;
using System;

namespace HoloFab {
    public class InteractionManager : Type_Manager<InteractionManager>
#if WINDOWS_UWP
		                            // , InputSystemGlobalHandlerListener
		                            , IMixedRealityPointerHandler
		                            // , IMixedRealityInputHandler
		                            // , IMixedRealityInputHandler<Vector2>
		                            // , IMixedRealityInputHandler<Vector3>
#endif
    {

        [Header("Actions")]
        public InputActionReference clickActionReference;
        public InputActionReference positionActionReference;

        //[Header("OLD")]
        //public float rotationSensitivity = .5f;

        //private Interactable_Movable[] movables;
        //[HideInInspector]
        //public Interactable_Movable activeMovable;


        //private Vector3 startDragPosition, currentDragPosition;
        //// Rotate
        //private Vector3 lastRelativeDrag;

        private void OnEnable() {
            this.clickActionReference.action.Enable();
            this.positionActionReference.action.Enable();
            EnableClickActions();
            EnableSelectActions();
        }
        private void OnDisable() {
            DisaableClickActions();
            DisableSelectActions();
        }
        private void Update() {
            CheckGaze();
        }

        #region GenericClickEvents
        // Click
        private bool flagClick = false;
        // Drag
        private bool flagDragStarted = false;
        private Vector2 startScreenDragPosition, currentScreenDragPosition;
        private Vector2 dragDifference {
            get {
                return this.startScreenDragPosition - this.currentScreenDragPosition;
            }
        }

        public delegate void onDragStart(Vector2 clickPosition);
        public delegate void onDragEvent(Vector2 difference);
        public onDragStart OnDragStart;
        public onDragEvent OnDragPerformed;
        public onDragEvent OnDragFinished;

        ////////////////////////////////////////////////////////////////////////
        private void EnableClickActions() {
            this.clickActionReference.action.started += OnClickStarted;
            this.clickActionReference.action.performed += OnClickPerformed;
            this.clickActionReference.action.canceled += OnClickFinished;
        }
        private void DisaableClickActions() {
            this.clickActionReference.action.started -= OnClickStarted;
            this.clickActionReference.action.performed -= OnClickPerformed;
            this.clickActionReference.action.canceled -= OnClickFinished;
        }
        private void OnClickStarted(InputAction.CallbackContext context) {
            this.flagClick = true;
            this.flagDragStarted = true;
            this.positionActionReference.action.performed += OnMovePerformed;
        }
        private void OnClickPerformed(InputAction.CallbackContext context) { }
        private void OnClickFinished(InputAction.CallbackContext context) {
            if (this.OnDragFinished != null)
                this.OnDragFinished.Invoke(this.dragDifference);
            this.flagClick = false;
            this.flagDragStarted = false;
            this.startScreenDragPosition = Vector2.zero;
            this.positionActionReference.action.performed -= OnMovePerformed;
        }
        private void OnMovePerformed(InputAction.CallbackContext context) {
            if (this.flagClick && this.flagDragStarted) {
                this.flagDragStarted = false;
                this.startScreenDragPosition = context.ReadValue<Vector2>();
                if (this.OnDragStart != null)
                    this.OnDragStart.Invoke(this.startScreenDragPosition);
            }
            this.currentScreenDragPosition = context.ReadValue<Vector2>();
            if (this.OnDragPerformed != null)
                this.OnDragPerformed.Invoke(this.dragDifference);
        }
        #endregion
        #region Placables
        public LayerMask gazableLayerMask;
        public LayerMask clickableLayerMask;
        // Gaze
        [HideInInspector]
        public RaycastHit hitGaze;
        [HideInInspector]
        public bool flagHitGaze = false;
        // Select
        [HideInInspector]
        public RaycastHit hitSelect;
        [HideInInspector]
        public bool flagHitSelect = false;

        private Vector2 interactorPosition;

        //private List<Interactable_Placeable> placeables = new List<Interactable_Placeable>();
        //[HideInInspector]
        //public Interactable_Placeable activePlaceable;

        public delegate bool onGameObjectClick(GameObject goHit);
        public onGameObjectClick OnGameObjectClick;
        public delegate void onClick();
        public onClick OnTap;

        //public void RegisterPlacable(Interactable_Placeable placable) {
        //    this.placeables.Add(placable);
        //}
        //public void UnregisterPlacable(Interactable_Placeable placable) {
        //    this.placeables.RemoveAt(this.placeables.IndexOf(placable));
        //}
        private void EnableSelectActions() {
            this.clickActionReference.action.canceled += OnTapped;
            this.clickActionReference.action.canceled += OnSelect;
            this.positionActionReference.action.performed += OnInteractorMoved;
        }
        private void DisableSelectActions()
        {
            this.clickActionReference.action.canceled -= OnTapped;
            this.clickActionReference.action.canceled -= OnSelect;
            this.positionActionReference.action.performed -= OnInteractorMoved;
        }
        private void OnTapped(InputAction.CallbackContext context) {
			#if DEBUG2
        	Debug.Log("InteractionManager: Tapped");
        	#endif
            if (this.OnTap != null) {
                this.OnTap.Invoke();
            }
        }
        private void CheckGaze() { 
        	// Send a ray and find if anything is being selected.
        	Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray.origin, ray.direction, out this.hitGaze, 
                float.MaxValue, this.gazableLayerMask)) {
        		#if DEBUG2
        		Debug.Log("Interaction Manager: Gaze hit gameObject: " + this.hitGaze.transform.gameObject.name);
        		#endif
        		this.flagHitGaze = true;
        	} else
        		this.flagHitGaze = false; // If nothing hit - reset history.
        }
        
        private void OnSelect(InputAction.CallbackContext context) {
            CheckSelect(this.interactorPosition);
        }
        private void OnInteractorMoved(InputAction.CallbackContext context) {
            this.interactorPosition = context.ReadValue<Vector2>();
        }
        private void CheckSelect(Vector2 clickPosition) {
            Ray ray = Camera.main.ScreenPointToRay(clickPosition);
            if (Physics.Raycast(ray.origin, ray.direction, out this.hitSelect,
                float.MaxValue, this.clickableLayerMask)) {
                #if DEBUG
                Debug.Log("Interaction Manager: Selection hit gameObject: " + this.hitSelect.transform.gameObject.name);
                #endif
                this.flagHitSelect = true;
                if (this.OnGameObjectClick != null) {
                    this.OnGameObjectClick.Invoke(this.hitSelect.transform.gameObject);
                }
            }
            else
                this.flagHitSelect = false; // If nothing hit - reset history.
        }
        #endregion

#if WINDOWS_UWP
		////////////////////////////////////////////////////////////////////////
		protected virtual void OnEnable(){
		RegisterHandlers();
		}

		protected virtual void OnDisable(){
		UnregisterHandlers();
		}
		private void RegisterHandlers() {
		// CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler>(this);
		CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
		}

		private void UnregisterHandlers() {
		// CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler>(this);
		CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
		}

		// public void OnPositionInputChanged(InputEventData<Vector2> eventData) {
		// 	Debug.Log("Interaction Manager: Input Position Changed vec 3 " + eventData.InputData);
		// }
		// public void OnPositionInputChanged(InputEventData<Vector3> eventData) {
		// 	Debug.Log("Interaction Manager: Input Position Changed Vec 2 " + eventData.InputData);
		// }
		//
		// public void OnInputChanged(InputEventData<Vector3> eventData) {
		// 	Debug.Log("Interaction Manager: Input Changed vec 3 " + eventData.InputData);
		// }
		//
		// public void OnInputChanged(InputEventData<Vector2> eventData) {
		// 	Debug.Log("Interaction Manager: Input Changed Vec 2 " + eventData.InputData);
		// }
		////////////////////////////////////////////////////////////////////////
		// public void OnInputDown(InputEventData eventData) {
		// 	Debug.Log("Interaction Manager: Input Down");
		// }
		// public void OnInputUp(InputEventData eventData) {
		// 	Debug.Log("Interaction Manager: Input Up");
		// }
		////////////////////////////////////////////////////////////////////////
		public void OnPointerClicked(MixedRealityPointerEventData eventData) {
#if DEBUG
		Debug.Log("Interaction Manager: OnPointer Clicked");
#endif
		// eventData.Use();
		}
		public void OnPointerDown(MixedRealityPointerEventData eventData) {
		this.currentDragPosition = eventData.Pointer.Position;
		Quaternion rotation = eventData.Pointer.Rotation;

#if DEBUG
		Debug.Log("Interaction Manager: OnPointer Down: Position " + this.currentDragPosition.ToString("F6") + ", rotation: " + rotation.ToString("F6"));
#endif
		// eventData.Use();

		this.flagClick = true;
		ExtractClickInfo();
		}
		public void OnPointerDragged(MixedRealityPointerEventData eventData) {
		this.currentDragPosition = eventData.Pointer.Position;
		Quaternion rotation = eventData.Pointer.Rotation;

#if DEBUG
		Debug.Log("Interaction Manager: OnPointer Drag: Position " + this.currentDragPosition.ToString("F6") + ", rotation: " + rotation.ToString("F6"));
#endif
		// eventData.Use();
		}

		public void OnPointerUp(MixedRealityPointerEventData eventData) {
		this.currentDragPosition = eventData.Pointer.Position;
		Quaternion rotation = eventData.Pointer.Rotation;

#if DEBUG
		Debug.Log("Interaction Manager: OnPointer Up: Position " + this.currentDragPosition.ToString("F6") + ", rotation: " + rotation.ToString("F6"));
#endif
		// eventData.Use();

		// Reset Dargging
		StopMoving();
		}
#endif
        //        ////////////////////////////////////////////////////////////////////////
        //        void Update()
        //        {
        //            // #if WINDOWS_UWP && DEBUG2
        //            // MixedRealityInputAction[] actions = CoreServices.InputSystem.InputSystemProfile.InputActionsProfile.InputActions;
        //            // if (actions.Length > 0)
        //            // 	Debug.Log("Interactible Manager: Mixed Reality events found: " + actions.Length);
        //            // #endif

        //            // Find what current selection is on
        //            CheckSelection();
        //            // Check if click has occured and process it to be handled by corresponding interactibles/
        //            CheckClick();
        //            // Check if dragging has finished.
        //            CheckEndDrag();

        //#if DEBUG
        //        	if (this.activeMovable != null)
        //        		Debug.Log("Interaction Manager: Active Movable: " + this.activeMovable.gameObject.name);
        //        	if (this.activePlaceable != null)
        //        		Debug.Log("Interaction Manager: Active Placeable: " + this.activePlaceable.gameObject.name);
        //#endif

        //            // Force unclick - cick handled
        //            this.flagClick = false;
        //        }
        ////////////////////////////////////////////////////////////////////////

        //        // A function to register clicks (cross platform).
        //        private void CheckClick()
        //        {
        //            // #if UNITY_ANDROID
        //            // if (Input.touchCount > 0) {
        //#if !WINDOWS_UWP
        //#if DEBUG2
        //        	Debug.Log("Touch: " + (Input.touchCount > 0) + ", Mouse: " + Input.GetMouseButtonDown(0));
        //#endif

        //            if (((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) || (Input.GetMouseButtonDown(0)))
        //            {
        //#if DEBUG2
        //        		if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began))
        //        			Debug.Log("Touch: " + (Input.GetTouch(0).position));
        //        		Debug.Log("Mouse: " + Input.mousePosition);
        //#endif
        //                this.flagClick = true;
        //                if (this.flagHitSelect || this.flagHitGaze)
        //                    ExtractClickInfo();
        //            }
        //#endif
        //            // In UWP handled by pointer events
        //        }
        //        // Extract information about cursor hit info.
        //        private void ExtractClickInfo()
        //        {
        //            // If clicked on scanned mesh - stop placment
        //            if ((this.activePlaceable != null) && (this.flagHitGaze && ObjectManager.instance.CheckEnvironmentObject(this.hitGaze.transform.gameObject)))
        //            {
        //            }
        //            else
        //            {
        //                //// Find interactibles if any.
        //                //if (this.flagHitSelect)
        //                //    this.activeMovable = CheckMovableHit(this.hitSelect.transform.gameObject);
        //            }
        //        }
        //        private void CheckEndDrag()
        //        {
        //#if !WINDOWS_UWP
        //            // Don't bother checking dragging if drag object wasn't found.
        //            if (this.activeMovable != null)
        //            {
        //                // Dragging taken care in Movable.
        //                // Only monitor stopping dragging.
        //                if (((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Ended)) || Input.GetMouseButtonUp(0))
        //                {
        //                    // Reset
        //                    StopMoving();
        //                }
        //            }
        //#endif
        //        }
        //        //////////////////////////////////////////////////////////////////////////
        //// Find Movable object that is hit (if any).
        //private Interactible_Movable CheckMovableHit(GameObject goHit)
        //{
        //    this.movables = FindObjectsOfType<Interactible_Movable>();
        //    foreach (Interactible_Movable movable in this.movables)
        //        if (movable.CheckTrigger(goHit))
        //            return movable;
        //    return null;
        //}
        //// Deactivate Movement on Active Mover.
        //private void StopMoving(bool flagForce = false)
        //{
        //    if (flagForce)
        //    {
        //        // Deactivate all movables - overkill, since we already have active one.
        //        this.movables = FindObjectsOfType<Interactible_Movable>();
        //        foreach (Interactible_Movable movable in this.movables)
        //            movable.StopMoving();
        //        this.activeMovable = null;
        //    }
        //    else if (this.activeMovable != null)
        //    {
        //        this.activeMovable.StopMoving();
        //        this.activeMovable = null;
        //    }
        //}
        //        ////////////////////////////////////////////////////////////////////////
        //        public Vector3 DragMoveDifference(bool flagDragStart)
        //        {
        //#if !WINDOWS_UWP
        //            this.currentDragPosition = CurrentProjectedPlanePoint(out bool _flagHit);
        //#endif

        //            if (flagDragStart)
        //                this.startDragPosition = this.currentDragPosition;
        //            return this.currentDragPosition - this.startDragPosition;
        //        }
        //        public float DragRotateDifference(bool flagDragStart)
        //        {
        //            Vector3 relativeDrag;
        //#if !WINDOWS_UWP
        //            this.currentDragPosition = CurrentProjectedPlanePoint(out bool _flagHit);
        //            relativeDrag = this.currentDragPosition - this.activeMovable.transform.position;
        //#else
        //            relativeDrag = this.currentDragPosition - this.startDragPosition;
        //#endif

        //            if (flagDragStart)
        //                this.lastRelativeDrag = relativeDrag;

        //            // a trick to check direction of rotation?
        //            // TODO: Should be done once?
        //            Vector3 controlVector = Quaternion.AngleAxis(1, this.activeMovable.orientationPlane.normal) * this.lastRelativeDrag;
        //            float currentAngle = Vector3.Angle(this.lastRelativeDrag, relativeDrag);
        //            float controlAngle = Vector3.Angle(controlVector, relativeDrag);

        //            if (controlAngle > currentAngle) currentAngle *= -1;

        //            this.lastRelativeDrag = relativeDrag;

        //            return currentAngle;//angleDifference * this.rotationSensitivity;
        //        }
        //        private Vector3 CurrentProjectedPlanePoint(out bool _flagHit)
        //{
        //    Ray cameraMouseRay = UnityUtilities.GenerateSelectionRay();//Camera.main.ScreenPointToRay(Input.mousePosition);
        //    // NB! Isn't it the same as mouse Ray
        //    if (this.activeMovable.orientationPlane.Raycast(cameraMouseRay, out float rayDistance))
        //    {
        //        _flagHit = true;
        //        return cameraMouseRay.GetPoint(rayDistance);
        //    }
        //    _flagHit = false;
        //    return Vector3.zero;
        //}
    }
}