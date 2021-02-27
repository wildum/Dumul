using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using QDollarGestureRecognizer;

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

    private InputActionMap _actionMap;
    private InputAction _inputActionTrigger;
    private InputAction _inputActionGrip;
    private InputAction _inputActionPosition;
    private InputAction _inputActionRotation;

    private Animator _handAnimator;

    private Control grip = new Control();
    private Control trigger = new Control();

    private TrailRenderer trailRenderer;

    private List<Point> points = new List<Point>();
    private Vector3 firstMovementPoint;
    private int strokeId = 0;
    private CustomRecognizerData customRecognizerData;

    private List<Vector3> shieldPoints = new List<Vector3>();

    private HandSideEnum side;

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
        if (trailType == TrailType.AttackSpell)
        {
            points.Clear();
        }
        else
        {
            shieldPoints.Clear();
        }
        strokeId = 0;
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

    public List<Point> getPoints()
    {
        return points;
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
    }

    private void OnDisable()
    {
        _inputActionGrip.Disable();
        _inputActionTrigger.Disable();
        _inputActionPosition.Disable();
        _inputActionRotation.Enable();
    }

    void Update()
    {
        trigger.setValue(_inputActionGrip.ReadValue<float>());
        grip.setValue(_inputActionTrigger.ReadValue<float>());

        _handAnimator.SetFloat("Grip", trigger.getValue());
        _handAnimator.SetFloat("Trigger", grip.getValue());

        Quaternion rotation = _inputActionRotation.ReadValue<Quaternion>();

        if (grip.pressing())
        {
            Vector3 position = _inputActionPosition.ReadValue<Vector3>();
            if (points.Count == 0)
            {
                firstMovementPoint = position;
                points.Add(new Point(0, position.y, strokeId));
                customRecognizerData.points = new List<Vector3>();
                customRecognizerData.rotations = new List<float>();
            }
            else
            {
                // compute the distance with the first point of the movement to reduce the dimension
                float d = Tools.dist2d(position.x, firstMovementPoint.x, position.z, firstMovementPoint.z);
                points.Add(new Point(d, position.y, strokeId));
            }
            customRecognizerData.points.Add(transform.position);
            customRecognizerData.rotations.Add(rotation.eulerAngles.y);
        }
        else if (trigger.pressing())
        {
            shieldPoints.Add(transform.position);
        }
    }

}
