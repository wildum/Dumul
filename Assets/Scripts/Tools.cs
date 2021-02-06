using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static class Tools
{
    public static float dist2d(float ax, float bx, float ay, float by)
    {
        return Mathf.Sqrt((bx-ax)*(bx-ax)+(by-ay)*(by-ay));
    }
}