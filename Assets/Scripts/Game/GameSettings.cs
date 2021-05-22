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

struct PlayerInfo
{
    public StartPosition position;
    public int team;
    public int id;
}


static class GameSettings
{
    public static int PLAYER_HEALTH = 1000;
    public static int nbPlayers = 2;
    public static float timeBeforeStart = 10.0f;
    public const float endGameTimer = 10.0f;
    public const float gameTimer = 300.0f;
    public const float pauseTimePopup = 5.0f;

    public static State currentState;

    public static int aiPositionCount = 0;

    private static List<StartPosition> startPos2Players = new List<StartPosition> {
        new StartPosition(new Vector3(-5, 0, 0), new Vector3(0, 90, 0)),
        new StartPosition(new Vector3(5, 0, 0), new Vector3(0, -90, 0))
    };
    
    private static List<StartPosition> startPos4Players = new List<StartPosition> {
        new StartPosition(new Vector3(-5, 0, 1), new Vector3(0, 90, 0)),
        new StartPosition(new Vector3(-5, 0, -1), new Vector3(0, 90, 0)),
        new StartPosition(new Vector3(5, 0, 1), new Vector3(0, -90, 0)),
        new StartPosition(new Vector3(5, 0, -1), new Vector3(0, -90, 0))
    };

    public static PlayerInfo getPlayerInfo(int id)
    {
        Dictionary<int, Photon.Realtime.Player> dict = PhotonNetwork.CurrentRoom.Players;
        List<int> actorNumbers = new List<int>();
        foreach (KeyValuePair<int, Photon.Realtime.Player> entry in dict)
        {
            actorNumbers.Add(entry.Value.ActorNumber);
        }

        actorNumbers.Sort();

        PlayerInfo playerInfo;
        playerInfo.team = 0;
        playerInfo.position = new StartPosition();
        playerInfo.id = 0;
        
        State currentState = State.Pratice;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("currentState"))
        {
            currentState = (State) PhotonNetwork.CurrentRoom.CustomProperties["currentState"];
        }
        else
        {
            Debug.LogError("snh current state of the room not set");
        }

        for (int i = 0; i < actorNumbers.Count; i++)
        {
            if (actorNumbers[i] == id)
            {
                // playing against AI the team is always 0
                if (currentState == State.TwoVsAI)
                {
                    playerInfo.team = 0;
                }
                else
                {
                    // little trick here to assign the teams
                    // if 1v1 then it is 0/2 = 0 => team 0, 1/2 = 0.5 => team 1
                    // if 2v2 then it is 0/4 = 0 => team 0, 1/4 = 0.25 => team 0, 2/4 = 0.5 => team 1, 3/4 = 0.75 => team 1
                    playerInfo.team = (i /(float) actorNumbers.Count) < 0.4 ? 0 : 1;
                }
                

                playerInfo.position = nbPlayers <= 2 ? startPos2Players[i] : startPos4Players[i];
                playerInfo.id = i;
                return playerInfo;
            }
        }

        Debug.LogError("team unassigned");
        return playerInfo;
    }

    public static StartPosition getStartPositionAI()
    {
        // hack so that the startPos4Players is 2 and then 3
        aiPositionCount++;
        return nbPlayers <= 2 ? startPos2Players[1] : startPos4Players[aiPositionCount + 1];
    }
}
