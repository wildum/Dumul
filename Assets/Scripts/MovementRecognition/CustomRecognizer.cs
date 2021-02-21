using UnityEditor;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;

public struct CustomRecognizerData
{
    public CustomRecognizerData(List<Vector3> p, float r)
    {
        points = p;
        rotation = r;
    }
    public List<Vector3> points { get; set; }
    public float rotation { get; set; }
}

public struct CustomRecognizerResult
{
    public CustomRecognizerResult(SpellEnum sp, float sc)
    {
        spell = sp;
        score = sc;
    }
    public SpellEnum spell { get; set; }
    public float score { get; set; }
}

public class CustomRecognizer : ScriptableObject
{
    static private List<CustomGesture> candidates;

    public static void init(List<CustomGestureIOData> data)
    {
        foreach (CustomGestureIOData d in data)
        {
            candidates.Add(new CustomGesture(d));
        }
    }

    public static CustomRecognizerResult classify(CustomGesture gesture)
    {
        CustomRecognizerResult result = new CustomRecognizerResult(SpellEnum.UNDEFINED, float.MaxValue);
        foreach (CustomGesture candidate in candidates)
        {
            float score = eval(gesture, candidate);
            if (score < result.score)
            {
                result.score = score;
                result.spell = candidate.getSpell();
            }
        }
        return result;
    }

    // TODO : to optimize this we could stop whener sum > threshold
    private static float eval(CustomGesture gesture, CustomGesture candidate)
    {
        float sum = 0;
        for (int i = 0; i < CustomGesture.SAMPLE_NUMBER; i++)
        {
            sum += Tools.dist3dPoints(gesture.getCustomPoints()[i], candidate.getCustomPoints()[i]);
        }
        return sum / CustomGesture.SAMPLE_NUMBER;
    }
}