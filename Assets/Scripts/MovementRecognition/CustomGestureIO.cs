using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Xml;

public struct CustomGestureIOData
{
    public CustomGestureIOData(List<Vector3> p, SpellRecognition s)
    {
        points = p;
        spell = s;
    }
    public List<Vector3> points { get; set; }
    public SpellRecognition spell { get; set; }
}

public class CustomGestureIO : ScriptableObject
{
    private static Dictionary<string, SpellRecognition> gestureNameToSpellEnum = new Dictionary<string, SpellRecognition> {
        { "fireball", SpellRecognition.Fireball },
        { "thunder", SpellRecognition.Thunder },
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
        List<Vector3> points = new List<Vector3>();
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
                        points.Add(new Vector3(
                            float.Parse(xmlReader["X"], System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(xmlReader["Y"], System.Globalization.CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(xmlReader["Z"], System.Globalization.CultureInfo.InvariantCulture.NumberFormat)
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