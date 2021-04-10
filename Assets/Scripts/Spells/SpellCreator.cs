using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpellCreator
{
    private int team = -1;

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
        return PhotonNetwork.Instantiate("Grenade", new Vector3(mx / points.Count, my / points.Count, mz / points.Count), Quaternion.identity);
    }

    public void createCross(Transform head, List<CustomPoint> p1, List<CustomPoint> p2)
    {
        Vector3 position = Tools.foundClosestMiddlePointBetweenTwoLists(p1, p2);
        Quaternion quaternion = head.rotation;
        quaternion.eulerAngles = new Vector3(0, quaternion.eulerAngles.y, 0);
        Vector3 direction = new Vector3(head.forward.x, 0, head.forward.z);
        GameObject cross = PhotonNetwork.Instantiate("Cross", position, quaternion);
        cross.GetComponent<Rigidbody>().AddForce(direction * Cross.CROSS_SPEED, ForceMode.VelocityChange);
        cross.GetComponent<Cross>().setTeam(team);
    }

    public void createFireball(List<CustomPoint> points)
    {
        Vector3 direction = computeFireballDirection(points);
        createFireballWithDirection(points, direction);
    }

    
    public void createFireballWithDirection(List<CustomPoint> points, Vector3 direction)
    {   
        Vector3 position = points[points.Count - 1].toVector3();
        GameObject fireball = PhotonNetwork.Instantiate("Fireball", position, Quaternion.identity);
        fireball.GetComponent<Rigidbody>().AddForce(direction.normalized * Fireball.FIREBALL_SPEED, ForceMode.Force);
        fireball.GetComponent<Fireball>().setTeam(team);
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
        PhotonNetwork.Instantiate("Thunder", position, rotation);
    }

    public int Team { set {team = value;}}

}