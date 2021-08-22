using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpectatorsManager : MonoBehaviour
{
    const float excitedTime = 2; // seconds
    const float hysterese = 1.5f; // seconds
    const float probExcitementBase = 0.02f;
    const float probExcitementMax = 0.1f;
    const float freqExcitement = 1;
    const float excitementDecaySecond = 0.75f;
    const float cheeringThreshold = 0.5f;
    const float booingThresholdDiff = -0.5f;

    public GameObject prefab;
    public Material matBlue;
    public Material matOrange;

    public static int team = -1;
    public SpectatorsAudio spectatorsAudio;

    private List<Spectator> blueSpectators = new List<Spectator>();
    private List<Spectator> orangeSpectators = new List<Spectator>();

    private float[] zPos = {-12.131f, -12.616f, -13.1f, -13.5f, -13.87f, -13.987f, -14.115f, -14.184f, -14.184f, -14.184f, -14.184f,
                            -14.184f, -14.184f, -14.184f, -14.115f, -13.987f, -13.87f, -13.5f, -13.1f, -12.616f, -12.131f};
    private int specPerLine = 21;
    private int specPerColumn = 5;
    private int sides = 2;

    private float probExcitementBlue = probExcitementBase;
    private float probExcitementOrange = probExcitementBase;

    private float totalOrangeHealth;
    private float totalBlueHealth;

    private float maxHealthDiff;
    private float excitementLevelBlue = 0;
    private float excitementLevelOrange = 0;

    private float time = 0;

    private bool gameEnded = false;

    private static System.Random rnd = new System.Random();

    void Start()
    {
        State currentState = State.Practice;
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(menu.RoomsHandler.stateProperty))
        {
            currentState = (State) PhotonNetwork.CurrentRoom.CustomProperties[menu.RoomsHandler.stateProperty];
        }
        else
        {
            Debug.LogError("current state of the room not set");
        }

        if (currentState != State.Practice)
        {
            initSpectators();
            initHealth(currentState);
            spectatorsAudio.startBackSound();
        }
        Destroy(prefab);
    }

    void Update()
    {
        time += Time.deltaTime;

        excitementLevelBlue = decayExcitement(excitementLevelBlue);
        excitementLevelOrange = decayExcitement(excitementLevelOrange);

        if (time > freqExcitement && !gameEnded)
        {
            time = 0;

            probExcitementBlue = computeExcitementProb(excitementLevelBlue);
            probExcitementOrange = computeExcitementProb(excitementLevelOrange);

            excitSpectators(blueSpectators, probExcitementBlue);
            excitSpectators(orangeSpectators, probExcitementOrange);

            Debug.Log("Blue excitement : " + excitementLevelBlue + " Orange : " + excitementLevelOrange);

            if (team == 0) handleAudio(excitementLevelOrange, excitementLevelBlue);
            else handleAudio(excitementLevelBlue, excitementLevelOrange);
        }
    }

    private void handleAudio(float myExcitement, float enemyExcitement)
    {
        if (myExcitement > cheeringThreshold)
            spectatorsAudio.startCheering();
        else if (myExcitement - enemyExcitement < booingThresholdDiff)
            spectatorsAudio.startBooing();
    }

    public void endGameExcitement(int winningTeam)
    {
        gameEnded = true;
        if (winningTeam == 0)
        {
            exciteTeam(orangeSpectators, true);
            exciteTeam(blueSpectators, false);
        }
        else if (winningTeam == 1)
        {
            exciteTeam(orangeSpectators, false);
            exciteTeam(blueSpectators, true);
        }
        else
        {
            exciteTeam(orangeSpectators, false);
            exciteTeam(blueSpectators, false);
        }

        if (winningTeam == team)
        {
            spectatorsAudio.startCheeringEndGame();
        }
        else
        {
            spectatorsAudio.startBooingEndGame();
        }
    }

    private void exciteTeam(List<Spectator> spectators, bool positifExictement)
    {
        foreach (Spectator spec in spectators)
        {
            if (positifExictement)
                spec.setExcitedState(GameSettings.endGameTimer);
            else
                spec.stopExcitement();
        }
    }

    private float computeExcitementProb(float excitementLevel)
    {
        return probExcitementBase + probExcitementMax * excitementLevel * 3;
    }

    private float decayExcitement(float excitement)
    {
        return excitement - (excitement - excitement * excitementDecaySecond) * Time.deltaTime;
    }

    private void computeExcitementLevel(float blueDiff, float orangeDiff)
    {
        excitementLevelOrange += Mathf.Sqrt(blueDiff / maxHealthDiff);
        excitementLevelBlue += Mathf.Sqrt(orangeDiff / maxHealthDiff);
    }

    public void currentTeamHealthUpdate(float orangeTeam, float blueTeam)
    {
        float orangeDiff = totalOrangeHealth - orangeTeam;
        float blueDiff = totalBlueHealth - blueTeam;
        totalBlueHealth = blueTeam;
        totalOrangeHealth = orangeTeam;
        computeExcitementLevel(blueDiff, orangeDiff);
    }

    void excitSpectators(List<Spectator> spectators, float probExcitement)
    {
        foreach (Spectator s in spectators)
        {
            if (rnd.NextDouble() < probExcitement)
            {
                s.setExcitedState(excitedTime + ((float) rnd.NextDouble() - 0.5f) * hysterese * 2.0f);
            }
        }
    }

    void initSpectators()
    {
        for (int i = 0; i < specPerLine; i++)
        {
            for (int j = 0; j < specPerColumn; j++)
            {
                for (int side = 0; side < sides; side++)
                {
                    GameObject obj = Instantiate(prefab, new Vector3(prefab.transform.position.x - 1.5f * i,
                        prefab.transform.position.y - 1.5f*j,
                        zPos[i] * (side == 0 ? 1.0f : -1.0f)),
                        Quaternion.identity);

                    obj.transform.Rotate(0, 90 * (side == 0 ? -1 : 1), 0);

                    Spectator s = obj.GetComponent<Spectator>();
                    int team = rnd.NextDouble() < 0.5 ? 0 : 1;
                    s.Team = team;
                    if (team == 0)
                        orangeSpectators.Add(s);
                    else
                        blueSpectators.Add(s);
                    foreach(var child in obj.GetComponentsInChildren<Renderer>())
                        child.material = team == 0 ? matOrange : matBlue;
                }
            }
        }
    }

    void initHealth(State currentState)
    {
        int nbPlayers = currentState == State.TwoVsAI || currentState == State.TwoVsTwo ? 4 : 2;
        maxHealthDiff = 2 * GameSettings.PLAYER_HEALTH / nbPlayers;
        float totalHealth = nbPlayers == 2 ? GameSettings.PLAYER_HEALTH : GameSettings.PLAYER_HEALTH * 2;
        totalOrangeHealth = totalHealth;
        totalBlueHealth = totalHealth;
    }
}
