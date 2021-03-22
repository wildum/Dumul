using UnityEditor;
using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;

using System.IO;
using System.Threading.Tasks;

public struct CustomRecognizerData
{
    public CustomRecognizerData(List<CustomPoint> p, List<float> r)
    {
        points = p;
        rotations = r;
    }
    public List<CustomPoint> points { get; set; }
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

        //debugPrintInFile();
    }

    private static void debugPrintInFile()
    {
        StreamWriter file = new StreamWriter("points.txt");
        foreach(CustomGesture c in candidates)
        {
            if (c.getSpell() != SpellRecognition.Fireball)
                continue;
            file.WriteLine(c.getSpell().ToString());
            for (int i = 0; i < c.getCustomPoints().Length; i++)
            {
                CustomPoint p = c.getCustomPoints()[i];
                file.WriteLine("<Point X=\""+p.x+"\" Y=\""+p.y+"\" Z=\""+p.z+"\" SIDE=\"" + (p.side == HandSideEnum.Left ? 0 : 1) +"\"/>");
            }
        }
        file.Close();
    }

    public static CustomRecognizerResult classify(CustomGesture gesture)
    {
        CustomRecognizerResult result = new CustomRecognizerResult(SpellRecognition.UNDEFINED, float.MinValue);
        foreach (CustomGesture candidate in candidates)
        {
            // first compare score between candidate and gesture
            float score = eval(gesture, candidate);
            // then compare score between gesture and candidate
            //Debug.Log(score);
            //score += eval(candidate, gesture);
            if (score > result.score)
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
        for (int i = 0; i < CustomGesture.SAMPLE_NUMBER; i++)
        {
            // float minDist = float.MaxValue;
            // for (int j = 0; j < CustomGesture.SAMPLE_NUMBER; j++)
            // {
            //     minDist = Mathf.Min(Tools.dist3dPointsSquared(a.getCustomPoints()[i], b.getCustomPoints()[j]), minDist);
            // }
            // sum += minDist;
            sum += Tools.dist3dPointsSquared(a.getCustomPoints()[i], b.getCustomPoints()[i]);
        }

        // sum*sum / CustomGesture.SAMPLE_NUMBER
        return CustomGesture.SAMPLE_NUMBER / (CustomGesture.SAMPLE_NUMBER + sum*sum);
    }
}