using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeHand : MonoBehaviour
{
    List<CustomPoint> points = new List<CustomPoint>();
    Vector3 originPosition;
    int currentIndex = 0;

    float distanceTrigger = 0.1f;
    float handMovementSpeed = 4.0f;
    bool reset = true;
    bool movementFinished = true;

    private TrailRenderer trailRenderer;

    private void Start()
    {
        originPosition = transform.position;
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        trailRenderer.time = Mathf.Infinity;
    }

    private void Update()
    {
        if (points.Count > 0)
        {
            Vector3 currentTarget = computeTargetPosition();
            if (!movementFinished)
            {
                moveToNextPoint(currentTarget);
            }
        }
    }

    public void RestartMovement()
    {
        movementFinished = false;
    }

    private void moveToNextPoint(Vector3 currentTarget)
    {
        if (reset)
        {
            transform.position = originPosition;
            reset = false;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, currentTarget, Time.deltaTime * handMovementSpeed);
        }
    }

    private Vector3 computeTargetPosition()
    {
        Vector3 currentTarget = points[currentIndex].toVector3() + originPosition;
        if (hasReachCurrentTarget(currentTarget))
        {
            movementFinished = currentIndex == points.Count - 1;

            if (movementFinished)
            {
                gameObject.SetActive(false);
            }

            currentIndex = (currentIndex + 1) % points.Count;
            currentTarget = points[currentIndex].toVector3() + originPosition;
            if (currentIndex == 0)
            {
                reset = true;
                trailRenderer.emitting = false;
                trailRenderer.Clear();
            }
            // if target is 1, then it means that we reached 0
            else if (currentIndex == 1)
            {
                trailRenderer.emitting = true;
            }
        }
        return currentTarget;
    }

    private bool hasReachCurrentTarget(Vector3 currentTarget)
    {
        Debug.Log(Vector3.Distance(transform.position, currentTarget) - distanceTrigger);
        return Vector3.Distance(transform.position, currentTarget) < distanceTrigger;
    }

    public void loadPoints(SpellEnum spell, List<CustomPoint> ipoints)
    {
        points = new List<CustomPoint>();
        currentIndex = 0;
        reset = true;
        movementFinished = true;
        trailRenderer.emitting = false;
        trailRenderer.Clear();
        gameObject.SetActive(false);
        for (int i = 0; i < ipoints.Count; i++)
        {
            CustomPoint c = new CustomPoint(ipoints[i]);
            if (spell == SpellEnum.Cross)
            {
                c.y += 0.5f;
            }
            points.Add(c);
        }
    }

    public bool MovementFinished{ get { return movementFinished;}}
}