using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Scripts.UI;
using UnityEngine.InputSystem;


namespace Game.Scripts.LiveObjects
{
    public class InteractableZone : MonoBehaviour
    {
        private GameInputs _inputs;
        private float _pressStartTime;

        private void Start()
        {
            InitializeInputs();
        }

        private void InitializeInputs()
        {
            _inputs = new GameInputs();
            _inputs.Player.Enable();
            _inputs.Player.Interact.started += Interact_started; 
            _inputs.Player.Interact.canceled += Interact_canceled;
            _inputs.Player.Interact.performed += Interact_performed;

        }

        private void Interact_performed(InputAction.CallbackContext obj)
        {
            if (_inZone == true)
            {
                if (_keyState == KeyState.PressHold && _inHoldState == false)
                {
                    Debug.Log("168");
                    _inHoldState = true;



                    switch (_zoneType)
                    {
                        case ZoneType.HoldAction:
                            PerformHoldAction();
                            break;
                    }
                }
            }
        }

        private void Interact_canceled(InputAction.CallbackContext obj)
        {
            Debug.Log("cancelled called");
            if (_inZone == true)
            {


                if (_keyState == KeyState.PressHold)
                {
                    Debug.Log("183");
                    _inHoldState = false;
                    onHoldEnded?.Invoke(_zoneID);
                }

                if (_keyState == KeyState.PressOrHold)
                {
                    //press
                    switch (_zoneType)
                    {

                        case ZoneType.Action:
                            float pressDuration = Time.time - _pressStartTime;
                            if (_actionPerformed == false)
                            {
                                Debug.Log("IZ160: "+ obj.duration);
                                for(int i=0; i< Mathf.CeilToInt(pressDuration); i++)
                                {
                                    PerformAction();

                                }
                                _actionPerformed = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;
                    }
                }


            }
        }

        private void Interact_started(InputAction.CallbackContext obj)
        {
            if (_inZone == true)
            {

                if (_keyState != KeyState.PressHold)
                {
                    switch (_zoneType)
                    {
                        case ZoneType.Collectable:
                            if (_itemsCollected == false)
                            {
                                CollectItems();
                                _itemsCollected = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;


                        case ZoneType.Action:
                            if (_actionPerformed == false)
                            {
                                PerformAction();
                                _actionPerformed = true;
                                UIManager.Instance.DisplayInteractableZoneMessage(false);
                            }
                            break;

                    }
                }
                else if (_keyState == KeyState.PressOrHold)
                {
                    //press
                    switch (_zoneType)
                    {
                        case ZoneType.Action:
                            _pressStartTime = Time.time;
                            break;

                    }
                }



            }
        }

        private enum ZoneType
        {
            Collectable,
            Action,
            HoldAction
        }

        private enum KeyState
        {
            Press,
            PressHold,
            PressOrHold
        }

        [SerializeField]
        private ZoneType _zoneType;
        [SerializeField]
        private int _zoneID;
        [SerializeField]
        private int _requiredID;
        [SerializeField]
        [Tooltip("Press the (---) Key to .....")]
        private string _displayMessage;
        [SerializeField]
        private GameObject[] _zoneItems;
        private bool _inZone = false;
        private bool _itemsCollected = false;
        private bool _actionPerformed = false;
        [SerializeField]
        private Sprite _inventoryIcon;
        //[SerializeField]
        //private KeyCode _zoneKeyInput;
        [SerializeField]
        private KeyState _keyState;
        [SerializeField]
        private GameObject _marker;

        private bool _inHoldState = false;

        private static int _currentZoneID = 0;
        public static int CurrentZoneID
        { 
            get 
            { 
               return _currentZoneID; 
            }
            set
            {
                _currentZoneID = value; 
                         
            }
        }


        public static event Action<InteractableZone> onZoneInteractionComplete;
        public static event Action<int> onHoldStarted;
        public static event Action<int> onHoldEnded;

        private void OnEnable()
        {
            InteractableZone.onZoneInteractionComplete += SetMarker;

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _currentZoneID > _requiredID)
            {
                switch (_zoneType)
                {
                    case ZoneType.Collectable:
                        if (_itemsCollected == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the E key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the E key to collect");
                        }
                        break;

                    case ZoneType.Action:
                        if (_actionPerformed == false)
                        {
                            _inZone = true;
                            if (_displayMessage != null)
                            {
                                string message = $"Press the E key to {_displayMessage}.";
                                UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                            }
                            else
                                UIManager.Instance.DisplayInteractableZoneMessage(true, $"Press the E key to perform action");
                        }
                        break;

                    case ZoneType.HoldAction:
                        _inZone = true;
                        if (_displayMessage != null)
                        {
                            string message = $"Press the E key to {_displayMessage}.";
                            UIManager.Instance.DisplayInteractableZoneMessage(true, message);
                        }
                        else
                            UIManager.Instance.DisplayInteractableZoneMessage(true, $"Hold the E key to perform action");
                        break;
                }
            }
        }

        private void Update()
        {
        }
       
        private void CollectItems()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(false);
            }

            UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);

            CompleteTask(_zoneID);

            onZoneInteractionComplete?.Invoke(this);

        }

        private void PerformAction()
        {
            foreach (var item in _zoneItems)
            {
                item.SetActive(true);
            }

            if (_inventoryIcon != null)
                UIManager.Instance.UpdateInventoryDisplay(_inventoryIcon);
            
            onZoneInteractionComplete?.Invoke(this);
        }

        private void PerformHoldAction()
        {
            Debug.Log("222");
            UIManager.Instance.DisplayInteractableZoneMessage(false);
            onHoldStarted?.Invoke(_zoneID);
        }

        public GameObject[] GetItems()
        {
            return _zoneItems;
        }

        public int GetZoneID()
        {
            return _zoneID;
        }

        public void CompleteTask(int zoneID)
        {
            if (zoneID == _zoneID)
            {
                _currentZoneID++;
                onZoneInteractionComplete?.Invoke(this);
            }
        }

        public void ResetAction(int zoneID)
        {
            if (zoneID == _zoneID)
                _actionPerformed = false;
        }

        public void SetMarker(InteractableZone zone)
        {
            if (_zoneID == _currentZoneID)
                _marker.SetActive(true);
            else
                _marker.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _inZone = false;
                UIManager.Instance.DisplayInteractableZoneMessage(false);
            }
        }

        private void OnDisable()
        {
            InteractableZone.onZoneInteractionComplete -= SetMarker;
        }       
        
    }
}


