using System.Collections;
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


static class GameSettings
{
    public static int PLAYER_HEALTH = 1000;
    public static int nbPlayers = 2;
    public const float timeBeforeStart = 10.0f;
    public const float endGameTimer = 10.0f;
    public const float gameTimer = 300.0f;
    public const float pauseTimePopup = 5.0f;

    private static List<StartPosition> playersStartPos = new List<StartPosition> {
        new StartPosition(new Vector3(-5, 0, 0), new Vector3(0, 90, 0)),
        new StartPosition(new Vector3(5, 0, 0), new Vector3(0, -90, 0))
    };

    public static int getTeamWithId(int id)
    {
        Dictionary<int, Photon.Realtime.Player> dict = PhotonNetwork.CurrentRoom.Players;
        List<int> actorNumbers = new List<int>();
        foreach (KeyValuePair<int, Photon.Realtime.Player> entry in dict)
        {
            actorNumbers.Add(entry.Value.ActorNumber);
        }

        actorNumbers.Sort();

        for (int i = 0; i < actorNumbers.Count; i++)
        {
            if (actorNumbers[i] == id)
            {
                // little trick here to assign the teams
                // if 1v1 then it is 0/2 = 0 => team 0, 1/2 = 0.5 => team 1
                // if 2v2 then it is 0/4 = 0 => team 0, 1/4 = 0.25 => team 0, 2/4 = 0.5 => team 1, 3/4 = 0.75 => team 1
                return (i /(float) actorNumbers.Count) < 0.5 ? 0 : 1;
            }
        }

        Debug.Log("team unassigned");
        return -1;
    }

    public static StartPosition getStartPositionFromTeam(int team)
    {
        if (team < 0 || team >= playersStartPos.Count)
        {
            Debug.Log("Error, no start positions for id : " + team);
            return new StartPosition();
        }
        return playersStartPos[team];
    }
}
