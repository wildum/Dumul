using UnityEditor;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

static class CommunicationCenter
{

    private static List<NetworkPlayer> players = new List<NetworkPlayer>();

    // to be updated when more that two players
    public static void updateHealth()
    {
        if (players.Count == 0)
        {
            initPlayersList();
        }
        int p1Health = 0;
        int p2Health = 0;
        if (players.Count > 0)
        {
            p1Health = players[0].getHealth();
            if (players.Count > 1)
            {
                p2Health = players[1].getHealth();
                players[1].updateHealthInfoCanvas(p1Health);
            }
            else
            {
                Debug.Log("Only one player alive");
            }
            players[0].updateHealthInfoCanvas(p2Health);
        }
        else
        {
            Debug.Log("No players alive");
        }
    }

    private static void initPlayersList()
    {
        NetworkPlayer[] playersArray = GameObject.FindObjectsOfType<NetworkPlayer>();
        foreach(NetworkPlayer p in playersArray)
        {
            players.Add(p);
        }
    }
}