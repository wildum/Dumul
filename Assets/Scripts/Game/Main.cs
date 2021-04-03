using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class Main : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    private GameObject aiPlayerPrefab;

    public static bool gameStarted = false;
    public static bool gameEnded = false;
    public static bool gameAborted = false;
    public static bool missingPlayer = false;
    public InfoCanvas infoCanvas1;
    public InfoCanvas infoCanvas2;

    float startTime = 0f;
    float timeSinceStart = 0f;

    float endTime = 0f;

    bool loadingMenu = false;

    void Start()
    {
        resetGameState();
        setNbOfPlayers();
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
        SpellRecognizer.init();
        ruleModeSpecific();
        startTime = Time.time;
    }

    void resetGameState()
    {
        gameStarted = false;
        gameEnded = false;
        gameAborted = false;
        missingPlayer = false;
        InformationCenter.clearPlayers();
    }

    void ruleModeSpecific()
    {
        if (AppState.currentState == State.Pratice)
        {
            GameSettings.timeBeforeStart = 0;
        }
        else if (AppState.currentState == State.OneVsAI)
        {
            // AI is always in the enemy team
            StartPosition s = GameSettings.getStartPositionFromTeam(1);
            Quaternion rotation = Quaternion.Euler(s.rotation);
            aiPlayerPrefab = PhotonNetwork.Instantiate("AI Player", s.position, rotation);
        }
    }

    public override void OnLeftRoom()
    {
        AppState.currentState = State.NoRoom;
        if (spawnedPlayerPrefab != null)
            PhotonNetwork.Destroy(spawnedPlayerPrefab);
        SceneManager.LoadScene("Menu");
        base.OnLeftRoom();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        Debug.Log("player left room, update list");
        missingPlayer = true;
    }

    private void Update()
    {
        if (gameAborted)
        {
            if (!loadingMenu)
            {
                loadingMenu = true;
                PhotonNetwork.Destroy(spawnedPlayerPrefab);
                if (aiPlayerPrefab)
                    PhotonNetwork.Destroy(aiPlayerPrefab);
                PhotonNetwork.LeaveRoom();
            }
        }
        else
        {
            timeSinceStart = Time.time - startTime;
            if (!gameStarted)
            {
                if (timeSinceStart > GameSettings.timeBeforeStart)
                {
                    Debug.Log("Game is starting");
                    gameStarted = true;
                }
                else
                {
                    infoCanvas1.handleGameNotStarted(timeSinceStart);
                    infoCanvas2.handleGameNotStarted(timeSinceStart);
                }
            }

            if (AppState.currentState != State.Pratice && checkGameEnd())
            {
                endTime += Time.deltaTime;
                if (!loadingMenu && endTime > GameSettings.endGameTimer)
                {
                    loadingMenu = true;
                    AppState.currentState = State.Lobby;
                    PhotonNetwork.LoadLevel("Menu");
                }
            }
            else if (gameStarted)
            {
                infoCanvas1.handleGameStarted(timeSinceStart);
                infoCanvas2.handleGameStarted(timeSinceStart);
            }
        }
    }

    private void setNbOfPlayers()
    {
        if (AppState.currentState == State.OneVsOne || AppState.currentState == State.OneVsAI)
        {
            GameSettings.nbPlayers = 2;
        }
        else if (AppState.currentState == State.Pratice)
        {
            GameSettings.nbPlayers = 1;
        }
    }

    private bool checkGameEnd()
    {
        List<ArenaPlayer> players = InformationCenter.getPlayers();
        if (gameStarted && !gameEnded)
        {
            bool teamZeroWon = false;
            bool teamOneWon = false;
            bool teamZeroUp = false;
            bool teamOneUp = false;
            int playersTeamZeroHealth = 0;
            int playersTeamOneHealth = 0;
            foreach (var p in players)
            {
                if (p.getHealth() <= 0)
                {
                    if (p.Team == 0)
                    {
                        teamOneWon = true;
                    }
                    else
                    {
                        teamZeroWon = true;
                    }
                }
                else if (p.Team == 0)
                {
                    playersTeamZeroHealth += p.getHealth();
                    teamZeroUp = true;
                }
                else if (p.Team == 1)
                {
                    playersTeamOneHealth += p.getHealth();
                    teamOneUp = true;
                }
            }
            // game end because of time.
            // Team with more health wins
            if (timeSinceStart > GameSettings.gameTimer + GameSettings.timeBeforeStart)
            {
                gameEnded = true;
                if (playersTeamZeroHealth > playersTeamOneHealth)
                {
                    infoCanvas1.updateEndGameText(EndGameStatus.Victory);
                    infoCanvas2.updateEndGameText(EndGameStatus.Defeat);
                }
                else if (playersTeamOneHealth > playersTeamZeroHealth)
                {
                    infoCanvas1.updateEndGameText(EndGameStatus.Defeat);
                    infoCanvas2.updateEndGameText(EndGameStatus.Victory);
                }
                else
                {
                    infoCanvas1.updateEndGameText(EndGameStatus.Draw);
                    infoCanvas2.updateEndGameText(EndGameStatus.Draw);
                }
            }
            else
            {
                checkTeamWin(0, teamZeroWon, teamOneUp);
                if (!gameEnded)
                    checkTeamWin(1, teamOneWon, teamZeroUp);
            }
        }
        return gameEnded;
    }

    private void checkTeamWin(int teamNumber, bool teamWon, bool enemyTeamUp)
    {
        if (teamWon || !enemyTeamUp)
        {
            if (teamNumber == 0)
            {
                infoCanvas1.updateEndGameText(EndGameStatus.Victory);
                infoCanvas2.updateEndGameText(EndGameStatus.Defeat);
                gameEnded = true;
            }
            else
            {
                infoCanvas1.updateEndGameText(EndGameStatus.Defeat);
                infoCanvas2.updateEndGameText(EndGameStatus.Victory);
                gameEnded = true;
            }
        }
    }
}
