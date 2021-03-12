using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Main : MonoBehaviour
{
    private GameObject spawnedPlayerPrefab;
    void Start()
    {
        Debug.Log("Start game");
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
        SpellRecognizer.init();
    }

}
