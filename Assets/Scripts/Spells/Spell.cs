using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum SpellRecognition
{
    Fireball,
    Thunder,
    ThunderRight,
    ThunderLeft,
    Cross,
    CrossLeft,
    CrossRight,
    GrenadeRight,
    GrenadeLeft,
    DashRight,
    DashLeft,
    DashLeftOnePart,
    DashRightOnePart,
    LaserLeft,
    LaserRight,
    UNDEFINED
}

public enum SpellEnum
{
    Fireball,
    Thunder,
    Cross,
    Grenade,
    DashRight,
    DashLeft,
    Laser,
    UNDEFINED
}

public enum SpellCdEnum
{
    FireballRight,
    FireballLeft,
    Cross,
    Thunder,
    GrenadeLeft,
    GrenadeRight,
    DashRight,
    DashLeft,
    Shield,
    LaserLeft,
    LaserRight,
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

public class Spell : MonoBehaviour, IPunInstantiateMagicCallback
{
    protected float cd = 10f;
    protected int speed = 50;
    protected int damage = 150;
    protected int playerId = 0;
    protected int team = 0;

    virtual public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info){Debug.Log("method should be overrided");}

    protected ArenaPlayer getPlayerFromCollision(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            GameObject g = collision.transform.parent.gameObject;
            if (g != null)
            {
                GameObject gp = g.transform.parent.gameObject;
                if (gp != null)
                {
                    return gp.GetComponent<ArenaPlayer>();
                }
            }
        }
        return null;
    }

    protected ArenaPlayer getPlayerFromCollider(Collider collider)
    {
        if (collider.tag == "Player")
        {
            GameObject g = collider.transform.parent.gameObject;
            if (g != null)
            {
                GameObject gp = g.transform.parent.gameObject;
                if (gp != null)
                {
                   return gp.GetComponent<ArenaPlayer>();
                }
            }
        }
        return null;
    }

    public float Cd { get { return cd; } }
    public int Speed { get { return speed; } }
    public int Damage { get { return damage; } }
    public int PlayerId { get { return playerId; } set { playerId = value;} }
    public int Team { get { return team; } set { team  = value; }}
}

