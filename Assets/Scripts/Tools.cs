using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Tools
{
    public static float dist2d(float ax, float bx, float ay, float by)
    {
        return Mathf.Sqrt((bx-ax)*(bx-ax)+(by-ay)*(by-ay));
    }

    public static float dist3dPoints(CustomPoint p1, CustomPoint p2)
    {
        return Mathf.Sqrt(dist3dPointsSquared(p1, p2));
    }

    public static float dist3dPointsSquared(CustomPoint p1, CustomPoint p2)
    {
        return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z);
    }

    public static float dist3dVectorSquared(Vector3 p1, Vector3 p2)
    {
        return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z);
    }

    public static Vector3 middlePoint(Vector3 p, Vector3 q)
    {
        return new Vector3((p.x + q.x) / 2, (p.y + q.y) / 2, (p.z + q.z) / 2);
    }

    public static float middle(float ax, float bx)
    {
        return (ax + bx) / 2.0f;
    }

    public static Vector3 foundClosestMiddlePointBetweenTwoLists(List<Vector3> p1, List<Vector3> p2)
    {
        Vector3 closest = new Vector3();
        float minDist = float.MaxValue;
        foreach(Vector3 p in p1)
        {
            foreach(Vector3 q in p2)
            {
                float tmp = dist3dVectorSquared(p, q);
                if (tmp < minDist)
                {
                    minDist = tmp;
                    closest = middlePoint(p, q);
                }
            }
        }
        return closest;
    }
}