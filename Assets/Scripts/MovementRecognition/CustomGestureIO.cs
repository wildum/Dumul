using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Xml;

public struct CustomGestureIOData
{
    public CustomGestureIOData(List<CustomPoint> p, SpellRecognition s)
    {
        points = p;
        spell = s;
    }
    public List<CustomPoint> points { get; set; }
    public SpellRecognition spell { get; set; }
}

public class CustomGestureIO : ScriptableObject
{
    private static Dictionary<string, SpellRecognition> gestureNameToSpellEnum = new Dictionary<string, SpellRecognition> {
        { "fireball", SpellRecognition.Fireball },
        { "thunder", SpellRecognition.Thunder },
        { "thunderLeft", SpellRecognition.ThunderLeft },
        { "thunderRight", SpellRecognition.ThunderRight },
        { "cross", SpellRecognition.Cross },
        { "crossLeft", SpellRecognition.CrossLeft },
        { "crossRight", SpellRecognition.CrossRight }
    };
    public static CustomGestureIOData ReadGestureFromXML(string xml)
    {

        XmlTextReader xmlReader = null;
        CustomGestureIOData gesture = new CustomGestureIOData();

        try
        {
            xmlReader = new XmlTextReader(new StringReader(xml));
            gesture = ReadGesture(xmlReader);

        }
        finally
        {

            if (xmlReader != null)
                xmlReader.Close();
        }

        return gesture;
    }

    private static CustomGestureIOData ReadGesture(XmlTextReader xmlReader)
    {
        List<CustomPoint> points = new List<CustomPoint>();
        string gestureName = "";
        try
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element) continue;
                switch (xmlReader.Name)
                {
                    case "Gesture":
                        gestureName = xmlReader["Name"];
                        break;
                    case "Point":
                        points.Add(new CustomPoint(
                            float.Parse(xmlReader["X"], System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(xmlReader["Y"], System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(xmlReader["Z"], System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
                            int.Parse(xmlReader["SIDE"]) == 0 ? HandSideEnum.Left : HandSideEnum.Right
                        ));
                        break;
                }
            }
        }
        finally
        {
            if (xmlReader != null)
                xmlReader.Close();
        }
        return new CustomGestureIOData(points, gestureNameToSpellEnum.ContainsKey(gestureName) ? gestureNameToSpellEnum[gestureName] : SpellRecognition.UNDEFINED);
    }
}