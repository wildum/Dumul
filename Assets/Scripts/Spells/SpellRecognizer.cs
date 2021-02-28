using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

static class SpellRecognizer
{
    public const float SPELL_RECO_THRESHOLD = 15.0f;
    public const int SPELL_RECO_MIN_ELEMENT = 5;
    public const float SHIELD_RECO_DISTANCE = 10.0f;
    public const int SHIELD_RECO_MIN_ELEMENT = 8;

    public static void init()
    {
        List<CustomGestureIOData> gesturesIO = new List<CustomGestureIOData>();
        TextAsset[] customGesturesXml = Resources.LoadAll<TextAsset>("CustomGestureData/");
        foreach (TextAsset gestureXml in customGesturesXml)
            gesturesIO.Add(CustomGestureIO.ReadGestureFromXML(gestureXml.text));
        CustomRecognizer.init(gesturesIO);
    }

    public static SpellEnum recognize(CustomRecognizerData data)
    {
        if (data.points.Count < SPELL_RECO_MIN_ELEMENT)
        {
            Debug.Log("Not enough elements : " + data.points.Count);
            return SpellEnum.UNDEFINED;
        }

        CustomGesture customGesture = new CustomGesture(data);
        CustomRecognizerResult res = CustomRecognizer.classify(customGesture);

        if (res.score > SPELL_RECO_THRESHOLD)
        {
            Debug.Log("Unrecognized, best was : " + res.spell + " : " + res.score);
            return SpellEnum.UNDEFINED;
        }
            
        Debug.Log(res.spell + " : " + res.score);
        return res.spell;
    }

    public static bool recognizeShield(List<Vector3> points)
    {
        if (points.Count < SHIELD_RECO_MIN_ELEMENT)
        {
            Debug.Log("Not enough elements to recognize shield");
            return false;
        }
        return Vector3.Distance(points[0], points[points.Count-1]) < SHIELD_RECO_DISTANCE;
    }
}