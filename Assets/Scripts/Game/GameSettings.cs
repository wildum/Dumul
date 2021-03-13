using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public const float timeBeforeStart = 10.0f;
    public const float gameTimer = 300.0f;

    private static List<StartPosition> playersStartPos = new List<StartPosition> {
        new StartPosition(new Vector3(-5, 0, 0), new Vector3(0, 90, 0)),
        new StartPosition(new Vector3(5, 0, 0), new Vector3(0, -90, 0))
    };

    // update to assign teams for multiple players
    public static int getTeamWithId(int id)
    {
        return id == 1 ? 0 : 1;
    }

    public static StartPosition getStartPositionFromActorId(int id)
    {
        int index = id - 1;
        if (index < 0 || index >= playersStartPos.Count)
        {
            Debug.Log("Error, no start positions for id : " + id);
            return new StartPosition();
        }
        return playersStartPos[index];
    }
}
