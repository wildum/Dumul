using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shield : MonoBehaviourPunCallbacks
{
    const float SHIELD_MAX_HEALTH = 150.0f;
    private float health = SHIELD_MAX_HEALTH;
    private float MAX_ALPHA = 200.0f;

    void Start()
    {
        gameObject.SetActive(false);
    }

    [PunRPC]
    public void reset()
    {
        health = SHIELD_MAX_HEALTH;
        updateColor();
    }

    void OnCollisionEnter(Collision collision)
    {
        Fireball ball = collision.collider.GetComponent<Fireball>();
        if (ball != null)
        {
            takeDamage(SpellEnum.Fireball);
            if (health <= 0)
            {
                this.gameObject.SetActive(false);
            }
        }
    }

    [PunRPC]
    void takeDamage(SpellEnum spell)
    {
        if (spell == SpellEnum.Fireball)
        {
            health -= Fireball.DAMAGE;
        }
        updateColor();
    }

    void updateColor()
    {
        Color c = this.GetComponent<MeshRenderer>().material.color;
        c.a = health / MAX_ALPHA;
        this.GetComponent<MeshRenderer>().material.color = c;
    }

    [PunRPC]
    public void updateActive(bool active)
    {
        this.gameObject.SetActive(active);
    }

}
