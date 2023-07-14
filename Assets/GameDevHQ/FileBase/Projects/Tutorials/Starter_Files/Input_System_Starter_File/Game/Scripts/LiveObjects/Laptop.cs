using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;

namespace Game.Scripts.LiveObjects
{
    public class Laptop : MonoBehaviour
    {
        private GameInputs _inputs;
        [SerializeField]
        private Slider _progressBar;
        [SerializeField]
        private int _hackTime = 5;
        private bool _hacked = false;
        [SerializeField]
        private CinemachineVirtualCamera[] _cameras;
        private int _activeCamera = 0;
        [SerializeField]
        private InteractableZone _interactableZone;

        public static event Action onHackComplete;
        public static event Action onHackEnded;

        private void OnEnable()
        {
            InteractableZone.onHoldStarted += InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded += InteractableZone_onHoldEnded;
            InitializeInputs();
        }
        private void InitializeInputs()
        {
            _inputs = new GameInputs();
            _inputs.Player.Enable();

        }

        private void Update()
        {
            if (_hacked == true)
            {
                if (_inputs.Player.Interact.triggered)
                {
                    var previous = _activeCamera;
                    _activeCamera++;


                    if (_activeCamera >= _cameras.Length)
                        _activeCamera = 0;


                    _cameras[_activeCamera].Priority = 11;
                    _cameras[previous].Priority = 9;
                }

                if (_inputs.Player.Escape.triggered)
                {
                    _hacked = false;
                    onHackEnded?.Invoke();
                    ResetCameras();
                }
            }
        }

        void ResetCameras()
        {
            foreach (var cam in _cameras)
            {
                cam.Priority = 9;
            }
        }

        private void InteractableZone_onHoldStarted(int zoneID)
        {
            Debug.Log("laptop:69");
            if (zoneID == 3 && _hacked == false) //Hacking terminal
            {
                Debug.Log("laptop:72");
                _progressBar.gameObject.SetActive(true);
                Debug.Log("laptop:74");
                StartCoroutine(HackingRoutine());
                Debug.Log("laptop:76");
                onHackComplete?.Invoke();
                Debug.Log("laptop:78");
            }
        }

        private void InteractableZone_onHoldEnded(int zoneID)
        {
            if (zoneID == 3) //Hacking terminal
            {
                if (_hacked == true)
                    return;
                Debug.Log("88");
                StopAllCoroutines();
                _progressBar.gameObject.SetActive(false);
                _progressBar.value = 0;
                onHackEnded?.Invoke();
            }
        }

        
        IEnumerator HackingRoutine()
        {
            Debug.Log("99");
            while (_progressBar.value < 1)
            {
                Debug.Log("102");
                _progressBar.value += Time.deltaTime / _hackTime;
                Debug.Log("103");
                yield return new WaitForEndOfFrame();
            }

            //successfully hacked
            Debug.Log("109");
            _hacked = true;
            Debug.Log("111");
            _interactableZone.CompleteTask(3);

            //hide progress bar
            Debug.Log("115");
            _progressBar.gameObject.SetActive(false);

            //enable Vcam1
            Debug.Log("119");
            _cameras[0].Priority = 11;
        }
        
        private void OnDisable()
        {
            InteractableZone.onHoldStarted -= InteractableZone_onHoldStarted;
            InteractableZone.onHoldEnded -= InteractableZone_onHoldEnded;
        }
    }

}

