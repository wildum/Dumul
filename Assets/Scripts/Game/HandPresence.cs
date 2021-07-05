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
    public string actionNameJoystick;
    public string actionNameShield;
    public HandSideEnum side;

    public GameObject pausePopup;

    private InputActionMap _actionMap;
    private InputAction _inputActionTrigger;
    private InputAction _inputActionGrip;
    private InputAction _inputActionPosition;
    private InputAction _inputActionRotation;
    private InputAction _inputActionPause;
    private InputAction _inputActionJoystick;
    private InputAction _inputActionShield;

    private Control grip = new Control();
    private Control shield = new Control();
    private Control pause = new Control();
    private Vector2 joystickData = new Vector2(0,0);

    private TrailRenderer trailRenderer;

    private CustomRecognizerData customRecognizerData;

    private SpellRecognition loadedTwoHandsSpell = SpellRecognition.UNDEFINED;
    private List<CustomPoint> loadedPoints = new List<CustomPoint>();
    private float currentTimeTwoHandsSpell = 0.0f;

    bool pausePressed = false;
    private GameObject pausePopupInstantiated;
    private int team = -1;

    private float headAngley = 0.0f;

    void Awake()
    {
        // get all of our actions
        _actionMap = actionAsset.FindActionMap(controllerName);
        _inputActionGrip = _actionMap.FindAction(actionNameGrip);
        _inputActionTrigger = _actionMap.FindAction(actionNameTrigger);
        _inputActionPosition = _actionMap.FindAction("Position");
        _inputActionRotation = _actionMap.FindAction("Rotation");
        _inputActionShield = _actionMap.FindAction(actionNameShield);

        if (side == HandSideEnum.Left)
        {
            _inputActionPause = _actionMap.FindAction(actionNamePause);
            _inputActionJoystick = _actionMap.FindAction(actionNameJoystick);
        }

        //spawnedHandModel = Instantiate(handModelPrefab, transform);
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;
        trailRenderer.time = Mathf.Infinity;
    }

    public void startTrail(TrailType trailType)
    {
        trailRenderer.emitting = true;
        if (trailType == TrailType.AttackSpell)
        {
            Color c = GameSettings.getTeamColor(team);
            trailRenderer.material.color = new Color(c.r, c.g, c.g, 0.5f);
        }
    }

    public void stopTrail(TrailType trailType)
    {
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        if (trailType == TrailType.AttackSpell)
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
        return shield.computeState();
    }

    public ControlState computeGripState()
    {
        return grip.computeState();
    }

    public float getTriggerValue()
    {
        return shield.getValue();
    }

    public bool activateShield()
    {
        return shield.pressing();
    }

    public float getShieldValue()
    {
        return grip.getValue();
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
        _inputActionShield.Enable();
        if (side == HandSideEnum.Left)
        {
            _inputActionPause.Enable();
            _inputActionJoystick.Enable();
        }
    }

    private void OnDisable()
    {
        _inputActionGrip.Disable();
        _inputActionTrigger.Disable();
        _inputActionPosition.Disable();
        _inputActionRotation.Disable();
        _inputActionShield.Disable();
        if (side == HandSideEnum.Left)
        {
            _inputActionPause.Disable();
            _inputActionJoystick.Disable();
        }
    }

    void Update()
    {
        shield.setValue(_inputActionShield.ReadValue<float>());
        grip.setValue(_inputActionGrip.ReadValue<float>());

        if (side == HandSideEnum.Left)
        {
            pause.setValue(_inputActionPause.ReadValue<float>());

           joystickData = _inputActionJoystick.ReadValue<Vector2>();

            if (pause.pressing())
            {
                if (!pausePressed)
                {
                    pausePressed = true;
                    Quaternion rotationPausePopup = Quaternion.identity;
                    rotationPausePopup.eulerAngles = new Vector3(0, team == 0 ? 90 : -90, 0);
                    pausePopupInstantiated = Instantiate(pausePopup, new Vector3(0, 3, 0), rotationPausePopup);
                }
            }
            else if (pausePressed)
            {
                pausePressed = false;
                Destroy(pausePopupInstantiated);
            }
        }

        if (grip.pressing())
        {
            if (customRecognizerData.points == null || customRecognizerData.rotations == null)
            {
                customRecognizerData.points = new List<CustomPoint>();
                customRecognizerData.rotations = new List<float>();
            }
            customRecognizerData.points.Add(new CustomPoint(transform.position, side));
            customRecognizerData.rotations.Add(headAngley);
        }
    }

    public void resetTwoHandsLoadedSpell()
    {
        loadedTwoHandsSpell = SpellRecognition.UNDEFINED;
        loadedPoints.Clear();
        currentTimeTwoHandsSpell = 0.0f;
    }

    public SpellRecognition LoadedTwoHandsSpell { get { return loadedTwoHandsSpell; } set { loadedTwoHandsSpell = value; } }
    public float CurrentTimeTwoHandsSpell { get { return currentTimeTwoHandsSpell; } set { currentTimeTwoHandsSpell = value; } }
    public int Team { get { return team; } set { team = value; } }
    public List<CustomPoint> LoadedPoints { get { return loadedPoints; } set { loadedPoints = value; } }
    public float HeadAngley {get {return headAngley;} set {headAngley = value;}}
    public Vector2 JoystickData { get { return joystickData; } }

}
