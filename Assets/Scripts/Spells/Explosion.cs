using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Explosion : MonoBehaviour
{

    private int damage = 0;
    private float maxDeltaScale = 2.5f;
    private float duration = 1.0f;
    private float currentDeltaScale = 0.0f;
    private bool alive = true;

    private List<int> playerTouched = new List<int>();

    private void Update()
    {
        if (alive)
        {
            float amountOfDifferent = Time.deltaTime / duration;
            currentDeltaScale += amountOfDifferent;
            if (currentDeltaScale > maxDeltaScale)
            {
                PhotonNetwork.Destroy(gameObject);
                alive = false;
            }
            else
            {
                transform.localScale += new Vector3(amountOfDifferent, amountOfDifferent, amountOfDifferent);
                applyDamage();
            }
        }
    }

    private void applyDamage()
    {
        // scaled in an uniform way so we can just use x
        float colliderRad = GetComponent<SphereCollider>().radius * transform.localScale.x;
        foreach (NetworkPlayer p in InformationCenter.getPlayers())
        {
            float distCollisionHead = Vector3.Distance(gameObject.transform.position, p.getPosition()) - p.getScaledHeadRadius();
            if (!playerTouched.Contains(p.photonView.Owner.ActorNumber) &&  distCollisionHead < colliderRad)
            {
                p.photonView.RPC("takeDamage", RpcTarget.All, damage);
                playerTouched.Add(p.photonView.Owner.ActorNumber);
                Debug.Log("player touched");
            }
        }
    }

    public int Damage {get {return damage;} set {damage = value;}}
    public float MaxDeltaScale {get {return maxDeltaScale;} set {maxDeltaScale = value;}}
    public float Duration {get {return duration;} set {duration = value;}}
}
