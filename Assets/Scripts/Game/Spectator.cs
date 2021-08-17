using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spectator : MonoBehaviour
{
    public Transform lefthand;
    public Transform rightHand;

    const float maxMovement = 0.5f;
    const float movementSpeed = 2;
    const float movementSpeedHand = 5;
    private int team;
    private float time = 0;
    private float excitedTime = 0;
    private bool excited = false;
    private bool upDirection = true;
    private Vector3 initialPosition;
    private Vector3 movedPosition;

    private Vector3 leftHandPositionInit;
    private Vector3 rightHandPositionInit;

    private Vector3 leftHandMovedPosition;
    private Vector3 rightHandMovedPosition;

    void Start()
    {
        initialPosition = transform.position;
        movedPosition = new Vector3(initialPosition.x, initialPosition.y + maxMovement, initialPosition.z);
        leftHandPositionInit = lefthand.localPosition;
        rightHandPositionInit = rightHand.localPosition;
        leftHandMovedPosition = new Vector3(leftHandPositionInit.x, leftHandPositionInit.y + 0.5f, leftHandPositionInit.z + 0.1f);
        rightHandMovedPosition = new Vector3(rightHandPositionInit.x, rightHandPositionInit.y + 0.5f, rightHandPositionInit.z - 0.1f);
    }

    void Update()
    {
        if (excited)
        {
            lefthand.localPosition = Vector3.Lerp(lefthand.localPosition, leftHandMovedPosition, Time.deltaTime * movementSpeedHand);
            rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, rightHandMovedPosition, Time.deltaTime * movementSpeedHand);

            if (upDirection)
            {
                transform.position = Vector3.Lerp(transform.position, movedPosition, Time.deltaTime * movementSpeed);
                if (isAlmostEqualY(transform.position, movedPosition))
                {
                    upDirection = false;
                }
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * movementSpeed);
                if (isAlmostEqualY(transform.position, initialPosition))
                {
                    upDirection = true;
                }
            }

            time += Time.deltaTime;
            if (time > excitedTime)
            {
                time = 0;
                excited = false;
            }
        }
        else
        {
            lefthand.localPosition = Vector3.Lerp(lefthand.localPosition, leftHandPositionInit, Time.deltaTime * movementSpeedHand);
            rightHand.localPosition = Vector3.Lerp(rightHand.localPosition, rightHandPositionInit, Time.deltaTime * movementSpeedHand);

            if (!isAlmostEqualY(transform.position, initialPosition))
            {
                upDirection = true;
                transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * movementSpeed);
            }
        }
    }

    private bool isAlmostEqualY(Vector3 v, Vector3 u)
    {
        return Mathf.Abs(v.y - u.y) < 0.1f;
    }

    public void setExcitedState(float iexcitedTime)
    {
        excited = true;
        time = 0;
        excitedTime = iexcitedTime;
    }

    public void stopExcitement()
    {
        excited = false;
        time = 0;
        excitedTime = 0;
    }

    public int Team { set { team = value; } get { return team;} }
}
