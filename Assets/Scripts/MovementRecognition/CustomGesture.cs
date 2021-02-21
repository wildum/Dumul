using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

public class CustomPoint
{
    public float x, y, z;

    public CustomPoint(float ix, float iy, float iz)
    {
        x = ix;
        y = iy;
        z = iz;
    }
    public CustomPoint(Vector3 point)
    {
        x = point.x;
        y = point.y;
        z = point.z;
    }
}

public class CustomGesture : ScriptableObject
{
    public const int SAMPLE_NUMBER = 64;
    private List<CustomPoint> points;
    private CustomPoint[] cleanPoints = new CustomPoint[SAMPLE_NUMBER];
    private float rotation = 0.0f;
    private SpellEnum spell = SpellEnum.UNDEFINED;

    public CustomGesture(CustomGestureIOData data)
    {
        spell = data.spell;
        buildGesture(data.points);
    }

    public CustomGesture(CustomRecognizerData data)
    {
        rotation = data.rotation;
        buildGesture(data.points);
    }

    private void buildGesture(List<Vector3> points)
    {
        if (points.Count > 1)
        {
            buildPoints(points);
            translateToOrigin();
            rotatePointsAroundY();
            reSample();
            scale();
        }
    }

    public SpellEnum getSpell()
    {
        return spell;
    }

    public void setSpell(SpellEnum iname)
    {
        spell = iname;
    }

    public CustomPoint[] getCustomPoints()
    {
        return cleanPoints;
    }

    void translateToOrigin()
    {
        for (int i = 1; i < points.Count; i++)
        {
            points[i].x -= points[0].x;
            points[i].y -= points[0].y;
            points[i].z -= points[0].z;
        }
        points[0].x = 0;
        points[0].y = 0;
        points[0].z = 0;
    }

    void rotatePointsAroundY()
    {
        float s = Mathf.Sin((rotation * Mathf.PI) / 180);
        float c = Mathf.Cos((rotation * Mathf.PI) / 180);

        for (int i = 1; i < points.Count; i++)
        {
            points[i].x = points[i].x * c - points[i].z * s;
            points[i].z = points[i].x * s + points[i].z * c;
        }
    }

    private float PathLength(List<CustomPoint> points)
    {
        float length = 0;
        for (int i = 1; i < points.Count; i++)
            length += Tools.dist3dPoints(points[i - 1], points[i]);
        return length;
    }

    public CustomPoint[] reSample()
    {
        cleanPoints[0] = new CustomPoint(points[0].x, points[0].y, points[0].z);
        int numPoints = 1;

        float I = PathLength(points) / (SAMPLE_NUMBER - 1); // computes interval length
        float D = 0;
        for (int i = 1; i < points.Count; i++)
        {
            float d = Tools.dist3dPoints(points[i - 1], points[i]);
            if (D + d >= I)
            {
                CustomPoint firstPoint = points[i - 1];
                while (D + d >= I)
                {
                    // add interpolated point
                    float t = Math.Min(Math.Max((I - D) / d, 0.0f), 1.0f);
                    if (float.IsNaN(t)) t = 0.5f;
                    cleanPoints[numPoints++] = new CustomPoint(
                        (1.0f - t) * firstPoint.x + t * points[i].x,
                        (1.0f - t) * firstPoint.y + t * points[i].y,
                        (1.0f - t) * firstPoint.z + t * points[i].z
                    );

                    // update partial length
                    d = D + d - I;
                    D = 0;
                    firstPoint = cleanPoints[numPoints - 1];
                }
                D = d;
            }
            else D += d;
        }

        if (numPoints == SAMPLE_NUMBER - 1) // sometimes we fall a rounding-error short of adding the last point, so add it if so
            cleanPoints[numPoints++] = new CustomPoint(points[points.Count - 1].x, points[points.Count - 1].y, points[points.Count - 1].z);
        return cleanPoints;
    }

    void scale()
    {
        float minx = float.MaxValue, miny = float.MaxValue, minz = float.MaxValue, maxx = float.MinValue, maxy = float.MinValue, maxz = float.MinValue;
        for (int i = 0; i < cleanPoints.Length; i++)
        {
            if (minx > points[i].x) minx = points[i].x;
            if (miny > points[i].y) miny = points[i].y;
            if (maxx < points[i].x) maxx = points[i].x;
            if (maxy < points[i].y) maxy = points[i].y;
            if (maxz < points[i].z) maxz = points[i].z;
            if (minz > points[i].z) minz = points[i].z;
        }

        float scale = Math.Max(Math.Max(maxx - minx, maxy - miny), maxz - minz);
        if (scale == 0)
        {
            Debug.Log("SNH : scale = 0");
            return;
        }
        for (int i = 0; i < cleanPoints.Length; i++)
        {
            cleanPoints[i].x = points[i].x / scale;
            cleanPoints[i].y = points[i].y / scale;
            cleanPoints[i].z = points[i].z / scale;
        }
    }

    void buildPoints(List<Vector3> rawPoints)
    {
        points = new List<CustomPoint>();
        foreach (var v in rawPoints)
        {
            points.Add(new CustomPoint(v));
        }
    }
}