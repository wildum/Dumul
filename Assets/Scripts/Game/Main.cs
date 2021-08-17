using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class Main : MonoBehaviourPunCallbacks
{
    private GameObject spawnedPlayerPrefab;
    private List<GameObject> aiPlayerPrefabs = new List<GameObject>();

    public static bool gameStarted = false;
    public static bool gameEnded = false;
    public static bool gameAborted = false;
    public static bool missingPlayer = false;
    public InfoCanvas infoCanvas1;
    public InfoCanvas infoCanvas2;

    public SpectatorsManager spectatorsManager;
    public GameObject ArenaShield;

    float startTime = 0f;
    float timeSinceStart = 0f;

    float endTime = 0f;

    bool loadingMenu = false;

    private State currentState = State.OneVsOne;
    private ExitGames.Client.Photon.Hashtable myCustomProperties = new ExitGames.Client.Photon.Hashtable();

    void Start()
    {
        resetGameState();
        setCurrentState();
        setNbOfPlayers();
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
        SpellRecognizer.init();
        ruleModeSpecific();
        startTime = Time.time;
    }

    void setCurrentState()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(menu.RoomsHandler.stateProperty))
        {
            currentState = (State) PhotonNetwork.CurrentRoom.CustomProperties[menu.RoomsHandler.stateProperty];
        }
        else
        {
            Debug.LogError("current state of the room not set");
        }
        GameSettings.currentState = currentState;
    }

    void resetGameState()
    {
        gameStarted = false;
        gameEnded = false;
        gameAborted = false;
        missingPlayer = false;
        GameSettings.aiPositionCount = 0;
        InformationCenter.clearPlayers();
        CommunicationCenter.resetCommunicationCenter();

        // is this useful ?
        // preload prefabs
        Resources.LoadAll("Spells");
        // change this
        Resources.LoadAll("Materials");
        GameSettings.loadMaterials();
    }

    void ruleModeSpecific()
    {
        // the masterClient is responsible to create the AIs
        if (PhotonNetwork.IsMasterClient)
        {
            if (currentState == State.Practice)
            {
                GameSettings.timeBeforeStart = 0;
                spawnAiPlayer("PracticeTarget", 2, 0);
                spawnedPlayerPrefab.GetComponent<NetworkPlayer>().Immortal = true;
                aiPlayerPrefabs[0].GetComponent<ArenaPlayer>().Immortal = true;
            }
            else
            {
                GameSettings.timeBeforeStart = 10.0f;
                if (currentState == State.OneVsAI)
                {
                    spawnAiPlayer("AI Player", 2, 0);
                }
                else if (currentState == State.TwoVsAI)
                {
                    spawnAiPlayer("AI Player", 2, 0);
                    spawnAiPlayer("AI Player", 3, 1);
                }
            }
        }
    }

    private void spawnAiPlayer(string AIType, int id, int iidInTeam)
    {
        StartPosition s = GameSettings.getStartPositionAI();
        Quaternion rotation = Quaternion.Euler(s.rotation);
        GameObject g = PhotonNetwork.Instantiate(AIType, s.position, rotation);
        aiPlayerPrefabs.Add(g);
        if (AIType == "AI Player")
            g.GetComponent<AIPlayer>().setId(id, iidInTeam);
    }

    public override void OnLeftRoom()
    {
        // currentState = State.NoRoom;
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
                foreach (var aiPlayer in aiPlayerPrefabs)
                    PhotonNetwork.Destroy(aiPlayer);
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

            if (currentState != State.Practice && checkGameEnd())
            {
                ArenaShield.SetActive(false);
                endTime += Time.deltaTime;
                if (!loadingMenu && endTime > GameSettings.endGameTimer)
                {
                    loadingMenu = true;
                    PhotonNetwork.LeaveRoom();
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
        if (currentState == State.OneVsOne || currentState == State.OneVsAI || currentState == State.Practice)
        {
            GameSettings.nbPlayers = 2;
        }
        else if (currentState == State.TwoVsAI || currentState == State.TwoVsTwo)
        {
            GameSettings.nbPlayers = 4;
        }
    }

    private bool checkGameEnd()
    {
        List<ArenaPlayer> players = InformationCenter.getPlayers();
        if (gameStarted && !gameEnded)
        {
            int playersTeamZeroHealth = 0;
            int playersTeamOneHealth = 0;
            foreach (var p in players)
            {
                if (p.Team == 0)
                {
                    playersTeamZeroHealth += p.getHealth();
                }
                else if (p.Team == 1)
                {
                    playersTeamOneHealth += p.getHealth();
                }
            }
            int winningTeam = -1;
            // game end because of time.
            // Team with more health wins
            if (timeSinceStart > GameSettings.gameTimer + GameSettings.timeBeforeStart)
            {
                gameEnded = true;
                if (playersTeamZeroHealth > playersTeamOneHealth)
                {
                    infoCanvas1.updateEndGameText(EndGameStatus.Victory);
                    infoCanvas2.updateEndGameText(EndGameStatus.Defeat);
                    winningTeam = 0;
                }
                else if (playersTeamOneHealth > playersTeamZeroHealth)
                {
                    infoCanvas1.updateEndGameText(EndGameStatus.Defeat);
                    infoCanvas2.updateEndGameText(EndGameStatus.Victory);
                    winningTeam = 1;
                }
                else
                {
                    infoCanvas1.updateEndGameText(EndGameStatus.Draw);
                    infoCanvas2.updateEndGameText(EndGameStatus.Draw);
                    winningTeam = -1;
                }
            }
            else
            {
                if (playersTeamZeroHealth <= 0 && playersTeamOneHealth <= 0)
                {
                    infoCanvas1.updateEndGameText(EndGameStatus.Draw);
                    infoCanvas2.updateEndGameText(EndGameStatus.Draw);
                    gameEnded = true;
                    winningTeam = -1;
                }
                else if (playersTeamZeroHealth <= 0)
                {
                    infoCanvas1.updateEndGameText(EndGameStatus.Defeat);
                    infoCanvas2.updateEndGameText(EndGameStatus.Victory);
                    gameEnded = true;
                    winningTeam = 1;
                }
                else if (playersTeamOneHealth <= 0)
                {
                    infoCanvas1.updateEndGameText(EndGameStatus.Victory);
                    infoCanvas2.updateEndGameText(EndGameStatus.Defeat);
                    gameEnded = true;
                    winningTeam = 0;
                }
            }
            if (gameEnded)
                spectatorsManager.endGameExcitement(winningTeam);
        }
        return gameEnded;
    }
}
