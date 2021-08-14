using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Explosion : MonoBehaviour, IPunInstantiateMagicCallback
{

    private int damage = 0;
    private float maxDeltaScale = 2.5f;
    private float duration = 1.0f;
    private float currentDeltaScale = 0.0f;
    private bool alive = true;
    private int playerId;

    private List<int> playerTouched = new List<int>();

    
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        int team = (int) instantiationData[0];
        ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        Color teamColor = team == 0 ? new Color(1, 0.6f, 0.34f) : new Color(0.34f, 0.36f, 1);
        grad.SetKeys( new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(teamColor, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) } );
        col.color = grad;
    }

    private void Update()
    {
        if (alive)
        {
            float amountOfDifferent = Time.deltaTime / duration;
            currentDeltaScale += amountOfDifferent;
            if (currentDeltaScale > maxDeltaScale)
            {
                alive = false;
                PhotonView photonView = PhotonView.Get(this);
                if (photonView.IsMine)
                {
                    PhotonNetwork.Destroy(gameObject);
                }
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
        foreach (ArenaPlayer p in InformationCenter.getPlayers())
        {
            if (p != null && p.Alive)
            {
                float distCollisionHead = Vector3.Distance(gameObject.transform.position, p.getPosition()) - p.getScaledHeadRadius();
                if (!playerTouched.Contains(p.photonView.Owner.ActorNumber) &&  distCollisionHead < colliderRad)
                {
                    p.photonView.RPC("takeDamage", RpcTarget.All, damage, playerId);
                    playerTouched.Add(p.photonView.Owner.ActorNumber);
                }
            }
        }
    }

    public int Damage {get {return damage;} set {damage = value;}}
    public float MaxDeltaScale {get {return maxDeltaScale;} set {maxDeltaScale = value;}}
    public float Duration {get {return duration;} set {duration = value;}}
    public int PlayerId { get { return playerId; } set { playerId = value;}}
}
