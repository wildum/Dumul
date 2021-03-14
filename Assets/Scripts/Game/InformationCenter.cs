using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

static class InformationCenter
{
    private static List<NetworkPlayer> players = new List<NetworkPlayer>();

    public static void clearPlayers()
    {
        players.Clear();
    }
    public static  List<NetworkPlayer> getPlayers()
    {
        if (GameSettings.nbPlayers != players.Count)
        {
            updatePlayersList();
        }
        return players; 
    }
    public static void updatePlayersList()
    {
        NetworkPlayer[] playersArray = GameObject.FindObjectsOfType<NetworkPlayer>();
        players.Clear();
        foreach (NetworkPlayer p in playersArray)
        {
            players.Add(p);
        }
    }
    public static NetworkPlayer getFirstPlayerOppositeTeam(int team)
    {
        if (GameSettings.nbPlayers != players.Count)
        {
            updatePlayersList();
        }
        foreach (NetworkPlayer p in players)
        {
            if (p.Team != team && p.Alive)
            {
                return p;
            }
        }
        Debug.Log("No enemy player alive, return null");
        return null;
    }
}