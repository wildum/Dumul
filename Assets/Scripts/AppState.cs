using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public enum State
{
    NoRoom,
    WaitingRoom,
    OneVsOne,
    Practice,
    OneVsAI,
    Starter,
    TwoVsTwo,
    TwoVsAI
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
    // add this in the room properties
    public static AiDifficulty currentAiDifficulty = AiDifficulty.Easy;
}
