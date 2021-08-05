using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Laser : Spell
{
    public const float LASER_CD = 10f;
    public const float LASER_SCALE_SPEED = 4.0f;
    public const int LASER_DAMAGE = 5;
    public const float LASER_DURATION = 6.0f;
    public const float LASER_DAMAGE_TICK_FREQ = 0.5f;
    public const float LASER_TIME_SHIELD_DESTRUCTION = 2.0f;
    public const float yStartingPosition = 5f;

    private const float TIME_BEFORE_SPAWN = 0.5f;
    private float t = 0;
    private bool alived = true;
    private bool canScale = true;

    private HandPresence hand;
    
    private ArenaPlayer currentTarget;
    private Dictionary<int, float> damageTicks = new Dictionary<int, float>();

    private List<int> colliderIds = new List<int>();
    
    private Shield currentTargetShield;
    private float tShieldTime = 0.0f;

    void Awake()
    {
        cd = LASER_CD;
        speed = 0;
        damage = LASER_DAMAGE;
    }

    void FixedUpdate()
    {
        t += Time.deltaTime;
        if (alived && hand != null && handleLifeTime())
        {
            handleRotationAndPosition();
            handleScaling();
            handleDamageTicks();
            handleDamages();
        }
    }

    void handleDamageTicks()
    {
        List<int> keys = new List<int>(damageTicks.Keys);
        foreach(var key in keys)
        {
            damageTicks[key] += Time.deltaTime;
        }
        tShieldTime += Time.deltaTime;
    }

    void handleScaling()
    {
        if (currentTargetShield != null)
        {
            scaleOnSchield();
        }
        else if (t > TIME_BEFORE_SPAWN && canScale)
        {
            setScale(transform.parent.transform.localScale.y + Time.deltaTime * LASER_SCALE_SPEED);
        }
    }

    bool handleLifeTime()
    {
        if (t > LASER_DURATION || !hand.Channeling)
        {
            PhotonView photonView = PhotonView.Get(this);
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(transform.parent.gameObject);
            }
            alived = false;
        }
        return alived;
    }

    void handleRotationAndPosition()
    {
        transform.parent.position = hand.transform.position;
        Quaternion quaternion = hand.transform.rotation;
        float offset = hand.side == HandSideEnum.Left ? -90 : 90;
        quaternion.eulerAngles = new Vector3(quaternion.eulerAngles.x, quaternion.eulerAngles.y, quaternion.eulerAngles.z + offset);
        transform.parent.rotation = quaternion;
    }

    void handleDamages()
    {
        if (currentTarget != null)
        {
            if (damageTicks[currentTarget.Id] >= LASER_DAMAGE_TICK_FREQ)
            {
                currentTarget.photonView.RPC("takeDamage", RpcTarget.All, damage, playerId);
                damageTicks[currentTarget.Id] = 0;
            }
        }

        if (currentTargetShield != null)
        {
            if (tShieldTime >= LASER_DAMAGE_TICK_FREQ)
            {
                currentTargetShield.takeDamages(damage);
                tShieldTime = 0;
            }
        }
    }

    public void setHand(HandPresence ihand)
    {
        hand = ihand;
    }

    private void OnDestroy()
    {
        hand.Channeling = false;
    }

    private void OnTriggerExit(Collider other)
    {
        ReliableOnTriggerExit.NotifyTriggerExit(other, gameObject);
        PhotonView photonView = PhotonView.Get(this);
        if (photonView.IsMine)
        {
            if (other.tag == "Shield")
            {
                currentTargetShield = null;
            }
            else if (other.tag == "Player") 
            {
                currentTarget = null;
            }
            colliderIds.Remove(other.gameObject.GetInstanceID());
            canScale = colliderIds.Count == 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Hand")
        {
            ReliableOnTriggerExit.NotifyTriggerEnter(other, gameObject, OnTriggerExit);
            colliderIds.Add(other.gameObject.GetInstanceID());
            PhotonView photonView = PhotonView.Get(this);
            if (photonView.IsMine)
            {
                canScale = false;
                handleTarget(other);
                handleCollision(other);
            }
        }
    }

    void handleTarget(Collider collider)
    {
        ArenaPlayer player = getPlayerFromCollider(collider);
        if (player != null)
        {
            currentTarget = player;
            if (!damageTicks.ContainsKey(currentTarget.Id))
            {
                damageTicks[currentTarget.Id] = LASER_DAMAGE_TICK_FREQ;
            }
        }
    }

    void handleCollision(Collider collider)
    {
        if (collider.tag != "Shield")
        {
            Vector3 p = collider.ClosestPoint(transform.parent.position);
            float distance = Vector3.Distance(p, transform.parent.position) / 2.0f;
            setScale(distance + 0.01f);
        }
        else
        {
            if (currentTargetShield == null)
            {
                currentTargetShield = collider.gameObject.GetComponent<Shield>();
            }
            scaleOnSchield();
        }
    }

    void scaleOnSchield()
    {
        Vector3 laserToShieldCenter = currentTargetShield.transform.position - transform.parent.position;
        Vector3 projection = Vector3.Project(laserToShieldCenter, transform.parent.up);
        Vector3 projectionPoint = projection + transform.parent.position;
        // Debug.Log("Shield : " + currentTargetShield.transform.position + " hand : " + transform.parent.position + " proj : " + projectionPoint);
        float projToShield = Vector3.Distance(currentTargetShield.transform.position, projectionPoint);
        float projToShieldCorrection = Mathf.Min(1.0f, projToShield);
        float distToContact = Mathf.Sqrt(Shield.SHIELD_RADIUS * Shield.SHIELD_RADIUS  - projToShieldCorrection * projToShieldCorrection);
        // Debug.Log("dist[projP / contact] : " + distToContact + " / dist[projPoint / shield] : " + projToShield);
        Vector3 contactPoint = projectionPoint + Vector3.Normalize(transform.parent.position - projectionPoint) * distToContact;
        // Debug.Log( "Contactpoint : " + contactPoint);
        float distance = Vector3.Distance(contactPoint, transform.parent.position) / 2.0f;
        // Debug.Log(distance);
        setScale(distance + 0.01f);
    }

    void setScale(float value)
    {
        Vector3 v = transform.parent.transform.localScale;
        v.y = value;
        transform.parent.transform.localScale = v;
    }
}
