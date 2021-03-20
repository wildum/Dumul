using UnityEditor;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;

public struct CustomRecognizerData
{
    public CustomRecognizerData(List<Vector3> p, List<float> r)
    {
        points = p;
        rotations = r;
    }
    public List<Vector3> points { get; set; }
    public List<float> rotations { get; set; }
}

public struct CustomRecognizerResult
{
    public CustomRecognizerResult(SpellRecognition sp, float sc)
    {
        spell = sp;
        score = sc;
    }
    public SpellRecognition spell { get; set; }
    public float score { get; set; }
}

public class CustomRecognizer : ScriptableObject
{
    static private List<CustomGesture> candidates = new List<CustomGesture>();

    public static void init(List<CustomGestureIOData> data)
    {
        foreach (CustomGestureIOData d in data)
        {
            candidates.Add(new CustomGesture(d));
        }
    }

    public static CustomRecognizerResult classify(CustomGesture gesture)
    {
        CustomRecognizerResult result = new CustomRecognizerResult(SpellRecognition.UNDEFINED, float.MaxValue);
        foreach (CustomGesture candidate in candidates)
        {
            // first compare score between candidate and gesture
            float score = eval(gesture, candidate);
            // then compare score between gesture and candidate
            score += eval(candidate, gesture);
            if (score < result.score)
            {
                result.score = score;
                result.spell = candidate.getSpell();
            }
        }
        return result;
    }

    // TODO : to optimize this we could stop whener sum > threshold
    private static float eval(CustomGesture a, CustomGesture b)
    {
        float sum = 0;
        // string dbx = "";
        // string dby = "";
        // string dbz = "";
        for (int i = 0; i < CustomGesture.SAMPLE_NUMBER; i++)
        {
            float minDist = float.MaxValue;
            for (int j = 0; j < CustomGesture.SAMPLE_NUMBER; j++)
            {
                // minDist = Mathf.Min(Tools.dist3dPoints(a.getCustomPoints()[i], b.getCustomPoints()[j]), minDist);
                minDist = Mathf.Min(Tools.dist3dPointsSquared(a.getCustomPoints()[i], b.getCustomPoints()[j]), minDist);
            }
            sum += minDist;
            // dbx += a.getCustomPoints()[i].x + ", ";
            // dby += a.getCustomPoints()[i].y + ", ";
            // dbz += a.getCustomPoints()[i].z + ", ";
        }
        // Debug.Log(dbx);
        // Debug.Log(dby);
        // Debug.Log(dbz);
        return sum*sum / CustomGesture.SAMPLE_NUMBER;
    }
}