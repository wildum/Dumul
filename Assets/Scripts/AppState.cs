using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

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
}
