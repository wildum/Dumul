using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace menu
{
    public class NetworkPlayerMenuSpawner : MonoBehaviourPunCallbacks
    {
        private GameObject spawnedPlayerPrefab;

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player Menu", transform.position, transform.rotation);
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();
            PhotonNetwork.Destroy(spawnedPlayerPrefab);
        }
    }
}