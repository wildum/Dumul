using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpellCreator
{
    private int team = -1;
    private int playerId;

    public GameObject createGrenade(List<CustomPoint> points)
    {
        float mx = 0;
        float my = 0;
        float mz = 0;
        foreach(CustomPoint p in points)
        {
            mx += p.x;
            my += p.y;
            mz += p.z;
        }
        GameObject grenade = PhotonNetwork.Instantiate("Spells/Grenade", new Vector3(mx / points.Count, my / points.Count, mz / points.Count), Quaternion.identity, 0, new object[]{team});
        grenade.GetComponent<Grenade>().PlayerId = playerId;
        return grenade;
    }

    public void createLaser(HandPresence hand)
    {
        hand.Channeling = true;
        GameObject laser = PhotonNetwork.Instantiate("Spells/Laser", hand.transform.position, hand.transform.rotation, 0, new object[]{team});
        laser.GetComponentInChildren<Laser>().setHand(hand);
        laser.GetComponentInChildren<Laser>().PlayerId = playerId;
        laser.GetComponentInChildren<Laser>().Team = team;
    }

    public void createLaserAI(Transform trans, bool leftSide)
    {
        GameObject laser = PhotonNetwork.Instantiate("Spells/Laser", trans.position, trans.rotation, 0, new object[]{team});
        laser.GetComponentInChildren<Laser>().setAiParams(trans, true, leftSide);
        laser.GetComponentInChildren<Laser>().PlayerId = playerId;
        laser.GetComponentInChildren<Laser>().Team = team;
    }

    public void createCross(Transform head, List<CustomPoint> p1, List<CustomPoint> p2)
    {
        Vector3 position = Tools.foundClosestMiddlePointBetweenTwoLists(p1, p2);
        Quaternion quaternion = head.rotation;
        quaternion.eulerAngles = new Vector3(0, quaternion.eulerAngles.y, 0);
        Vector3 direction = new Vector3(head.forward.x, 0, head.forward.z);
        GameObject cross = PhotonNetwork.Instantiate("Spells/Cross", position, quaternion, 0, new object[]{team});
        cross.GetComponent<Rigidbody>().AddForce(direction * Cross.CROSS_SPEED, ForceMode.VelocityChange);
        cross.GetComponent<Cross>().Team = team;
        cross.GetComponent<Cross>().PlayerId = playerId;
    }

    public void createFireball(List<CustomPoint> points)
    {
        Vector3 direction = computeFireballDirection(points);
        createFireballWithDirection(points, direction);
    }

    public void createFireballWithDirection(List<CustomPoint> points, Vector3 direction)
    {   
        Vector3 position = points[points.Count - 1].toVector3();
        GameObject fireball = PhotonNetwork.Instantiate("Spells/Fireball", position, Quaternion.identity, 0, new object[]{team});
        fireball.GetComponent<Rigidbody>().AddForce(direction.normalized * Fireball.FIREBALL_SPEED, ForceMode.Force);
        ArenaPlayer target = InformationCenter.getRelevantPlayerOppositeTeam(direction, position, team);
        fireball.GetComponent<Fireball>().setTarget(target);
        fireball.GetComponent<Fireball>().PlayerId = playerId;
    }

    // expects at least 3 elements
    public Vector3 computeFireballDirection(List<CustomPoint> points)
    {
        return points[points.Count - 1].toVector3() - points[0].toVector3();
    }

    public void createThunder(Vector3 enemyPosition)
    {
        Vector3 position = new Vector3(enemyPosition.x, Thunder.yStartingPosition, enemyPosition.z);
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = new Vector3(0,0,180);
        GameObject thunder = PhotonNetwork.Instantiate("Spells/Thunder", position, rotation, 0, new object[]{team});
        thunder.GetComponentInChildren<Thunder>().PlayerId = playerId;
    }

    public void createDash(int direction)
    {
        Debug.Log("dash!");
        ArenaPlayer p = InformationCenter.getPlayerById(playerId);
        p.dash(direction);
    }

    public int Team { set {team = value;}}
    public int PlayerId { set { playerId = value; } get { return playerId; }}

}