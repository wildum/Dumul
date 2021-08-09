using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ThunderParent : MonoBehaviour, IPunInstantiateMagicCallback
{
    public void OnPhotonInstantiate(Photon.Pun.PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        int team = (int) instantiationData[0];
        GetComponentInChildren<Thunder>().Team = team;
        GetComponentInChildren<ParticleSystemRenderer>().trailMaterial = GameSettings.getElectricalMaterial(team);
    }
}