using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Xml;

public struct CustomGestureIOData
{
    public CustomGestureIOData(List<Vector3> p, SpellEnum s)
    {
        points = p;
        spell = s;
    }
    public List<Vector3> points { get; set; }
    public SpellEnum spell { get; set; }
}

public class CustomGestureIO : ScriptableObject
{
    private static Dictionary<string, SpellEnum> gestureNameToSpellEnum = new Dictionary<string, SpellEnum> {
        { "fireball", SpellEnum.Fireball },
        { "thunder", SpellEnum.Thunder }
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
                            float.Parse(xmlReader["X"]),
                            float.Parse(xmlReader["Y"]),
                            float.Parse(xmlReader["Z"])
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
        return new CustomGestureIOData(points, gestureNameToSpellEnum.ContainsKey(gestureName) ? gestureNameToSpellEnum[gestureName] : SpellEnum.UNDEFINED);
    }
}