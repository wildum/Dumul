using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using QDollarGestureRecognizer;

static class SpellRecognizer
{
    public const float SPELL_RECO_THRESHOLD = 20.0f;
    public const int SPELL_RECO_MIN_ELEMENT = 5;
    public const float SHIELD_RECO_DISTANCE = 10.0f;
    public const int SHIELD_RECO_MIN_ELEMENT = 8;
    private static List<Gesture> trainingSet = new List<Gesture>();

    public static void init()
    {
        //Load pre-made gestures
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

        //Load user custom gestures
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (string filePath in filePaths)
            trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));

        Debug.Log("SpellRecognizer initialised with " + trainingSet.Count + " elements");
    }

    public static SpellEnum recognize(List<Point> points)
    {

        if (points.Count < SPELL_RECO_MIN_ELEMENT)
        {
            Debug.Log("Not enough elements : " + points.Count);
            return SpellEnum.UNDEFINED;
        }

        Gesture candidate = new Gesture(points.ToArray());
        Result gestureResult = QPointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

        if (gestureResult.Score > SPELL_RECO_THRESHOLD)
        {
            Debug.Log("Unrecognized, best was : " + gestureResult.GestureClass + " with score of " + gestureResult.Score);
            return SpellEnum.UNDEFINED;
        }

        if (gestureResult.GestureClass == "line")
        {
            Debug.Log("Fireball with score of " + gestureResult.Score);
            return SpellEnum.Fireball;
        }
        Debug.Log("No spell mapped to this movement : " + gestureResult.GestureClass + " with score of " + gestureResult.Score);
        return SpellEnum.UNDEFINED;
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