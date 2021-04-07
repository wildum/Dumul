using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public enum State
{
    NoRoom,
    Lobby,
    OneVsOne,
    Pratice,
    OneVsAI,
    Starter
}

public enum AiDifficulty
{
    Easy,
    Medium,
    Hard
}

// TODO HANDLE THE STATE PROPERLY OVER THE NETWORK
public static class AppState
{
    public static State currentState = State.Starter;
    public static AiDifficulty currentAiDifficulty = AiDifficulty.Easy;

    [PunRPC]
    public static void setCurrentState(State state)
    {
        currentState = state;
    }
}
