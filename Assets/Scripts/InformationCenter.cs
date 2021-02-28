using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

static class InformationCenter
{
    private static List<NetworkPlayer> players = new List<NetworkPlayer>();
    public static  List<NetworkPlayer> getPlayers()
    {
        updatePlayersList();
        return players; 
    }
    private static void updatePlayersList()
    {
        NetworkPlayer[] playersArray = GameObject.FindObjectsOfType<NetworkPlayer>();
        foreach (NetworkPlayer p in playersArray)
        {
            players.Add(p);
        }
    }
    public static NetworkPlayer getFirstPlayerOppositeTeam(int team)
    {
        updatePlayersList();
        foreach (NetworkPlayer p in players)
        {
            if (p.Team != team && p.Alive)
            {
                return p;
            }
        }
        Debug.Log("No player alive, return null");
        return null;
    }
}