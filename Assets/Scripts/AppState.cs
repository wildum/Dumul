using UnityEditor;
using UnityEngine;

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
