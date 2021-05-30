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
