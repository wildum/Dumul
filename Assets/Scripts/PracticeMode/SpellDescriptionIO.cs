using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Xml;

public class SpellDescriptionIO : ScriptableObject
{
    private static Dictionary<string, SpellEnum> gestureNameToSpellEnum = new Dictionary<string, SpellEnum> {
        { "fireball", SpellEnum.Fireball },
        { "thunder", SpellEnum.Thunder },
        { "grenade", SpellEnum.Grenade },
        { "cross", SpellEnum.Cross },
        { "dash right", SpellEnum.DashRight },
        { "dash left", SpellEnum.DashLeft },
        { "laser", SpellEnum.Laser }
    };
    public static Dictionary<SpellEnum, string> ReadSpellDescriptionFromXML(string xml)
    {

        XmlTextReader xmlReader = null;
        Dictionary<SpellEnum, string> descriptions = new Dictionary<SpellEnum, string>();

        try
        {
            xmlReader = new XmlTextReader(new StringReader(xml));
            while (xmlReader.Read())
            {
                if (xmlReader.Name == "Spell")
                    descriptions.Add(gestureNameToSpellEnum[xmlReader["Name"]], xmlReader["Description"]);
            }
        }
        finally
        {
            if (xmlReader != null)
                xmlReader.Close();
        }

        return descriptions;
    }
}