using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

static class InformationCenter
{
    private static List<ArenaPlayer> players = new List<ArenaPlayer>();

    private static System.Random rnd = new System.Random();

    public static void clearPlayers()
    {
        players.Clear();
    }

    // can contain null if a player died
    public static List<ArenaPlayer> getPlayers()
    {
        if (GameSettings.nbPlayers != players.Count || Main.missingPlayer)
        {
            updatePlayersList();
        }
        return players; 
    }

    public static ArenaPlayer getPlayerById(int id)
    {
        if (GameSettings.nbPlayers != players.Count || Main.missingPlayer)
        {
            updatePlayersList();
        }

        foreach (var p in players)
        {
            if (p.Id == id)
                return p;
        }
        Debug.Log("id not found, player quit ?");
        return null; 
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

    public static ArenaPlayer getRandomPlayerOppositeTeam(int team)
    {
        if (GameSettings.nbPlayers != players.Count)
        {
            updatePlayersList();
        }
        List<ArenaPlayer> potentialTargets = new List<ArenaPlayer>();
        foreach (ArenaPlayer p in players)
        {
            if (p.Team != team && p.Alive)
            {
                potentialTargets.Add(p);
            }
        }
        if (potentialTargets.Count == 0)
        {
            Debug.Log("No enemy player alive, return null");
            return null;
        }
        else
        {
            return potentialTargets.Count == 1 ? potentialTargets[0] :  potentialTargets[rnd.NextDouble() < 0.5 ? 0 : 1];
        }
    }

    public static ArenaPlayer getRelevantPlayerOppositeTeam(Vector3 headDirection, Vector3 headPosition, int team)
    {
        if (GameSettings.nbPlayers != players.Count)
        {
            updatePlayersList();
        }
        float angleDistance = float.MaxValue;
        ArenaPlayer player = null;
        foreach (ArenaPlayer p in players)
        {
            if (p.Team != team && p.Alive)
            {
                Vector3 vHeadPlayer = p.getPosition() - headPosition;
                float angleValue = Mathf.Abs(Vector3.Angle(headDirection, vHeadPlayer));
                if (angleValue < angleDistance)
                {
                    angleDistance = angleValue;
                    player = p;
                }
            }
        }
        return player;
    }
}