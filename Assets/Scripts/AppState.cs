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
    Starter
}

public static class AppState
{
    public static State currentState = State.Starter;

    [PunRPC]
    public static void setCurrentState(State state)
    {
        currentState = state;
    }
}
