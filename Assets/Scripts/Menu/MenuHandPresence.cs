using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace menu
{
    public class MenuHandPresence : MonoBehaviour
    {
        public GameObject handModelPrefab;
        private GameObject spawnedHandModel;

        public InputActionAsset actionAsset;
        public string controllerName;
        public string actionNameTrigger;
        public string actionNameGrip;

        private InputActionMap _actionMap;
        private InputAction _inputActionTrigger;
        private InputAction _inputActionGrip;
        private InputAction _inputActionPosition;
        private InputAction _inputActionRotation;

        private Control grip = new Control();
        private Control trigger = new Control();

        private Animator _handAnimator;

        void Awake()
        {
            // get all of our actions
            _actionMap = actionAsset.FindActionMap(controllerName);
            _inputActionGrip = _actionMap.FindAction(actionNameGrip);
            _inputActionTrigger = _actionMap.FindAction(actionNameTrigger);
            _inputActionPosition = _actionMap.FindAction("Position");
            _inputActionRotation = _actionMap.FindAction("Rotation");

            spawnedHandModel = Instantiate(handModelPrefab, transform);
            _handAnimator = spawnedHandModel.GetComponent<Animator>();
        }

        private void OnEnable()
        {
            _inputActionGrip.Enable();
            _inputActionTrigger.Enable();
            _inputActionPosition.Enable();
            _inputActionRotation.Enable();
        }

        private void OnDisable()
        {
            _inputActionGrip.Disable();
            _inputActionTrigger.Disable();
            _inputActionPosition.Disable();
            _inputActionRotation.Enable();
        }

        public float getTriggerValue()
        {
            return trigger.getValue();
        }

        public float getShieldValue()
        {
            return grip.getValue();
        }

        void Update()
        {
            trigger.setValue(_inputActionGrip.ReadValue<float>());
            grip.setValue(_inputActionTrigger.ReadValue<float>());

            _handAnimator.SetFloat("Grip", trigger.getValue());
            _handAnimator.SetFloat("Trigger", grip.getValue());
        }
    }
}
