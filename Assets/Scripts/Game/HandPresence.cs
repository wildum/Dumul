using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TrailType
{
    AttackSpell,
    DefenseSpell
}

public enum HandSideEnum
{
    Left,
    Right
}

public class HandPresence : MonoBehaviour
{
    public GameObject handModelPrefab;
    private GameObject spawnedHandModel;

    public InputActionAsset actionAsset;
    public string controllerName;
    public string actionNameTrigger;
    public string actionNameGrip;
    public string actionNamePause;
    public HandSideEnum side;

    public GameObject pausePopup;

    private InputActionMap _actionMap;
    private InputAction _inputActionTrigger;
    private InputAction _inputActionGrip;
    private InputAction _inputActionPosition;
    private InputAction _inputActionRotation;
    private InputAction _inputActionPause;

    private Animator _handAnimator;

    private Control grip = new Control();
    private Control trigger = new Control();
    private Control pause = new Control();

    private TrailRenderer trailRenderer;

    private CustomRecognizerData customRecognizerData;

    private List<Vector3> shieldPoints = new List<Vector3>();

    private SpellEnum loadedTwoHandsSpell = SpellEnum.UNDEFINED;
    private float currentTimeTwoHandsSpell = 0.0f;

    bool pausePressed = false;
    private GameObject pausePopupInstantiated;

    void Awake()
    {
        // get all of our actions
        _actionMap = actionAsset.FindActionMap(controllerName);
        _inputActionGrip = _actionMap.FindAction(actionNameGrip);
        _inputActionTrigger = _actionMap.FindAction(actionNameTrigger);
        _inputActionPosition = _actionMap.FindAction("Position");
        _inputActionRotation = _actionMap.FindAction("Rotation");

        if (side == HandSideEnum.Left)
            _inputActionPause = _actionMap.FindAction(actionNamePause);

        spawnedHandModel = Instantiate(handModelPrefab, transform);
        _handAnimator = spawnedHandModel.GetComponent<Animator>();
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;
        trailRenderer.time = Mathf.Infinity;
    }

    public void startTrail(TrailType trailType)
    {
        trailRenderer.emitting = true;
        if (trailType == TrailType.AttackSpell)
        {
            trailRenderer.material.color = Color.red;
        }
        else
        {
            trailRenderer.material.color = Color.blue;
        }
    }

    public void stopTrail(TrailType trailType)
    {
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        if (trailType == TrailType.DefenseSpell)
        {
            shieldPoints.Clear();
        }
        else if (trailType == TrailType.AttackSpell)
        {
            customRecognizerData.points.Clear();
            customRecognizerData.rotations.Clear();
        }
    }

    public void setHandSide(HandSideEnum iSide)
    {
        side = iSide;
    }

    public HandSideEnum getHandSide()
    {
        return side;
    }

    public ControlState computeTriggerState()
    {
        return trigger.computeState();
    }

    public ControlState computeGripState()
    {
        return grip.computeState();
    }

    public float getTriggerValue()
    {
        return trigger.getValue();
    }

    public float getGripValue()
    {
        return grip.getValue();
    }

    public bool gripPressing()
    {
        return grip.pressing();
    }

    public List<Vector3> getShieldPoints()
    {
        return shieldPoints;
    }

    public CustomRecognizerData getCustomRecognizerData()
    {
        return customRecognizerData;
    }

    private void OnEnable()
    {
        _inputActionGrip.Enable();
        _inputActionTrigger.Enable();
        _inputActionPosition.Enable();
        _inputActionRotation.Enable();
        if (side == HandSideEnum.Left)
        {
            _inputActionPause.Enable();
        }
    }

    private void OnDisable()
    {
        _inputActionGrip.Disable();
        _inputActionTrigger.Disable();
        _inputActionPosition.Disable();
        _inputActionRotation.Disable();
        if (side == HandSideEnum.Left)
        {
            _inputActionPause.Disable();
        }
    }

    void Update()
    {
        trigger.setValue(_inputActionGrip.ReadValue<float>());
        grip.setValue(_inputActionTrigger.ReadValue<float>());

        if (side == HandSideEnum.Left)
        {
            pause.setValue(_inputActionPause.ReadValue<float>());
            if (pause.pressing())
            {
                if (!pausePressed)
                {
                    pausePressed = true;
                    pausePopupInstantiated = Instantiate(pausePopup, new Vector3(0, 3, 0), transform.rotation);
                }
            }
            else if (pausePressed)
            {
                pausePressed = false;
                Destroy(pausePopupInstantiated);
            }
        }

        _handAnimator.SetFloat("Grip", trigger.getValue());
        _handAnimator.SetFloat("Trigger", grip.getValue());

        Quaternion rotation = _inputActionRotation.ReadValue<Quaternion>();

        if (grip.pressing())
        {
            if (customRecognizerData.points == null || customRecognizerData.rotations == null)
            {
                customRecognizerData.points = new List<Vector3>();
                customRecognizerData.rotations = new List<float>();
            }
            customRecognizerData.points.Add(transform.position);
            customRecognizerData.rotations.Add(rotation.eulerAngles.y);
        }
        else if (trigger.pressing())
        {
            shieldPoints.Add(transform.position);
        }
    }

    public void resetTwoHandsLoadedSpell()
    {
        loadedTwoHandsSpell = SpellEnum.UNDEFINED;
        currentTimeTwoHandsSpell = 0.0f;
    }

    public SpellEnum LoadedTwoHandsSpell { get { return loadedTwoHandsSpell; } set { loadedTwoHandsSpell = value; } }
    public float CurrentTimeTwoHandsSpell { get { return currentTimeTwoHandsSpell; } set { currentTimeTwoHandsSpell = value; } }

}
