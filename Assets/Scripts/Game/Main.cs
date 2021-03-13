using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Main : MonoBehaviour
{
    private GameObject spawnedPlayerPrefab;

    public static bool gameStarted = false;
    public InfoCanvas infoCanvas1;
    public InfoCanvas infoCanvas2;

    float startTime = 0f;

    void Start()
    {
        Debug.Log("Start game");
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
        SpellRecognizer.init();
        startTime = Time.time;
    }

    private void Update()
    {
        float timeSinceStart = Time.time - startTime;
        if (!gameStarted)
        {
            if (timeSinceStart > GameSettings.timeBeforeStart)
            {
                gameStarted = true;
            }
            else
            {
                infoCanvas1.handleGameNotStarted(timeSinceStart);
                infoCanvas2.handleGameNotStarted(timeSinceStart);
            }
        }
        if (gameStarted)
        {
            infoCanvas1.handleGameStarted(timeSinceStart);
            infoCanvas2.handleGameStarted(timeSinceStart);
        }
    }

}
