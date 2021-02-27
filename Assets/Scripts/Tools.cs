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
        return Mathf.Sqrt((p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z));
    }

    public static float dist3dPointsSquared(CustomPoint p1, CustomPoint p2)
    {
        return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z);
    }

    public static float middle(float ax, float bx)
    {
        return (ax + bx) / 2.0f;
    }
}