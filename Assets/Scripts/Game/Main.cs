using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Main : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;

    public static bool gameStarted = false;
    public static bool gameEnded = false;
    public InfoCanvas infoCanvas1;
    public InfoCanvas infoCanvas2;

    float startTime = 0f;
    float timeSinceStart = 0f;

    float endTime = 0f;

    bool loadingMenu = false;

    void Start()
    {
        gameStarted = false;
        gameEnded = false;
        setNbOfPlayers();
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
        SpellRecognizer.init();
        startTime = Time.time;
    }

    private void Update()
    {
        timeSinceStart = Time.time - startTime;
        if (!gameStarted)
        {
            if (timeSinceStart > GameSettings.timeBeforeStart)
            {
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

    private void setNbOfPlayers()
    {
        if (AppState.currentState == State.OneVsOne)
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
        List<NetworkPlayer> players = InformationCenter.getPlayers();
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
