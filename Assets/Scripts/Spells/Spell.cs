using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpellRecognition
{
    Fireball,
    Thunder,
    CrossLeft,
    CrossRight,
    UNDEFINED
}

public enum SpellEnum
{
    Fireball,
    Thunder,
    Cross,
    UNDEFINED
}

public enum SpellCdEnum
{
    FireballRight,
    FireballLeft,
    Cross,
    Thunder,
    UNDEFINED
}

public class SpellCd
{
    private float cd;
    private float currentCd;
    private string name;

    public SpellCd(float icd, string iname)
    {
        cd = icd;
        currentCd = icd;
        name = iname;
    }

    public float Cd { get { return cd; } }
    public float CurrentCd { get { return currentCd; } set { currentCd = value; } }
    public string Name { get { return name; } }
}

public class Spell : MonoBehaviour
{
    protected float cd = 10f;
    protected int speed = 50;
    protected int damage = 150;

    protected NetworkPlayer getPlayerFromCollision(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            GameObject g = collision.transform.parent.gameObject;
            if (g != null)
            {
                GameObject gp = g.transform.parent.gameObject;
                if (gp != null)
                {
                    return gp.GetComponent<NetworkPlayer>();
                }
            }
        }
        return null;
    }

    public float Cd { get { return cd; } }
    public int Speed { get { return speed; } }
    public int Damage { get { return damage; } }
}

