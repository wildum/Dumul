using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

static class InformationCenter
{
    private static List<ArenaPlayer> players = new List<ArenaPlayer>();

    public static void clearPlayers()
    {
        players.Clear();
    }
    public static  List<ArenaPlayer> getPlayers()
    {
        if (GameSettings.nbPlayers != players.Count || Main.missingPlayer)
        {
            updatePlayersList();
        }
        return players; 
    }
    public static void updatePlayersList()
    {
        ArenaPlayer[] playersArray = GameObject.FindObjectsOfType<ArenaPlayer>();
        players.Clear();
        foreach (ArenaPlayer p in playersArray)
        {
            players.Add(p);
        }
    }
    public static ArenaPlayer getFirstPlayerOppositeTeam(int team)
    {
        if (GameSettings.nbPlayers != players.Count)
        {
            updatePlayersList();
        }
        foreach (ArenaPlayer p in players)
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