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
        foreach (var particles in GetComponentsInChildren<ParticleSystemRenderer>())
        {
            particles.trailMaterial = GameSettings.getElectricalMaterial(team);
        }
        foreach (var ps in GetComponentsInChildren<ParticleSystem>())
        {
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient grad = new Gradient();
            Color teamColor = team == 0 ? new Color(1, 0.6f, 0.34f) : new Color(0.34f, 0.36f, 1);
            grad.SetKeys( new GradientColorKey[] { new GradientColorKey(teamColor, 1.0f), new GradientColorKey(Color.white, 0.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) } );
            col.color = grad;
        }
    }
}