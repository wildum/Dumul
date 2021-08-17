﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

class StartPosition
{
    public Vector3 position;
    public Vector3 rotation;

    public StartPosition()
    {
        position = new Vector3(0,0,0);
        rotation = new Vector3(0,0,0);
    }

    public StartPosition(Vector3 pos, Vector3 ori)
    {
        position = pos;
        rotation = ori;
    }
}

struct PlayerInfo
{
    public StartPosition position;
    public int team;
    public int id;
    public int idInTeam;
}


static class GameSettings
{
    public static int PLAYER_HEALTH = 1000;
    public static int nbPlayers = 2;
    public static float timeBeforeStart = 10.0f;
    public const float endGameTimer = 7.0f;
    public const float gameTimer = 300.0f;
    public const float pauseTimePopup = 3.0f;

    public static State currentState;

    public static int aiPositionCount = 0;

    private static StartPosition startPos2PlayersOne = new StartPosition(new Vector3(-5, 0, 0), new Vector3(0, 90, 0));
    private static StartPosition startPos2PlayersTwo = new StartPosition(new Vector3(5, 0, 0), new Vector3(0, -90, 0));
    
    private static StartPosition startPos4PlayersOneOne = new StartPosition(new Vector3(-5, 0, 1), new Vector3(0, 90, 0));
    private static StartPosition startPos4PlayersTwoOne = new StartPosition(new Vector3(-5, 0, -1), new Vector3(0, 90, 0));
    private static StartPosition startPos4PlayersOneTwo = new StartPosition(new Vector3(5, 0, 1), new Vector3(0, -90, 0));
    private static StartPosition startPos4PlayersTwoTwo = new StartPosition(new Vector3(5, 0, -1), new Vector3(0, -90, 0));

    private static Color colorTeam1 = new Color(1.0f, 0.6f, 0.27f);
    private static Color colorTeam2 = new Color(0.39f, 0.64f, 0.73f);

    private static Material electricalBlue;
    private static Material electricalOrange;

    public static Color getTeamColor(int team)
    {
        return team == 0 ? colorTeam1 : colorTeam2;
    }

    public static void loadMaterials()
    {
        electricalBlue = Resources.Load<Material>("Materials/ElectricalEffectBlue");
        electricalOrange = Resources.Load<Material>("Materials/ElectricalEffectOrange");
        Grenade.orangeShellMat = Resources.Load<Material>("Materials/GrenadeOrange");
        Grenade.blueShellMat = Resources.Load<Material>("Materials/GrenadeBlue");
        Grenade.orangeTrailMat = Resources.Load<Material>("Materials/GrenadeTrailOrange");
        Grenade.blueTrailMat = Resources.Load<Material>("Materials/GrenadeTrailBlue");
    }

    public static Material getElectricalMaterial(int team)
    {
        return team == 0 ? electricalOrange : electricalBlue;
    }

    public static PlayerInfo getPlayerInfo(int id)
    {
        Dictionary<int, Photon.Realtime.Player> dict = PhotonNetwork.CurrentRoom.Players;
        List<Photon.Realtime.Player> actors = new List<Photon.Realtime.Player>();
        PlayerInfo playerInfo;
        playerInfo.team = 0;
        playerInfo.position = new StartPosition();
        playerInfo.id = 0;
        playerInfo.idInTeam = 0;
        int countFirstTeam = 0;
        int countSecondTeam = 0;

        foreach (KeyValuePair<int, Photon.Realtime.Player> entry in dict)
        {
            actors.Add(entry.Value);
            PhotonTeam team = entry.Value.GetPhotonTeam();
            if (entry.Value.ActorNumber == id)
                playerInfo.team = team.Code - 1;
        }

        actors.Sort((p, q) => p.ActorNumber.CompareTo(q.ActorNumber));

        for (int i = 0; i < actors.Count; i++)
        {
            PhotonTeam team = actors[i].GetPhotonTeam();
            if (team.Code == 1)
                countFirstTeam++;
            else
                countSecondTeam++;
            if (actors[i].ActorNumber == id)
            {
                if (nbPlayers <= 2)
                {
                    if (playerInfo.team == 0)
                        playerInfo.position = startPos2PlayersOne;
                    else
                        playerInfo.position = startPos2PlayersTwo;
                    playerInfo.idInTeam = 0;
                }
                else
                {
                    if (playerInfo.team == 0)
                    {
                        playerInfo.position = countFirstTeam == 1 ? startPos4PlayersOneOne : startPos4PlayersTwoOne;
                        playerInfo.idInTeam = countFirstTeam-1;
                    }
                    else
                    {
                        playerInfo.position = countSecondTeam == 1 ? startPos4PlayersOneTwo : startPos4PlayersTwoTwo;
                        playerInfo.idInTeam = countSecondTeam-1;
                    }
                }
                playerInfo.id = i;
                return playerInfo;
            }
        }

        Debug.LogError("team unassigned");
        return playerInfo;
    }

    public static StartPosition getStartPositionAI()
    {
        aiPositionCount++;
        return nbPlayers <= 2 ? startPos2PlayersTwo : aiPositionCount == 1 ? startPos4PlayersOneTwo : startPos4PlayersTwoTwo;
    }
}
