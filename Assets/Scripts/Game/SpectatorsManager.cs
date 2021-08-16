using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorsManager : MonoBehaviour
{
    const float exitedTime = 5; // seconds
    const float hysterese = 2; // seconds
    const float probExitementBase = 0.2f;
    const float freqExitement = 5;

    public GameObject prefab;
    public Material matBlue;
    public Material matOrange;

    private List<Spectator> blueSpectators = new List<Spectator>();
    private List<Spectator> orangeSpectators = new List<Spectator>();

    private float[] zPos = {-12.131f, -12.616f, -13.1f, -13.5f, -13.87f, -13.987f, -14.115f, -14.184f, -14.184f, -14.184f, -14.184f,
                            -14.184f, -14.184f, -14.184f, -14.115f, -13.987f, -13.87f, -13.5f, -13.1f, -12.616f, -12.131f};
    private int specPerLine = 21;
    private int specPerColumn = 5;
    private int sides = 2;

    private float probExitementBlue = probExitementBase;
    private float probExitementOrange = probExitementBase;

    private float time = 0;

    private static System.Random rnd = new System.Random();

    void Start()
    {
        initSpectators();
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time > freqExitement)
        {
            time = 0;
            exitSpectators(blueSpectators, probExitementBlue);
            exitSpectators(orangeSpectators, probExitementOrange);
        }
    }

    void exitSpectators(List<Spectator> spectators, float probExitement)
    {
        foreach (Spectator s in spectators)
        {
            if (rnd.NextDouble() < probExitement)
            {
                s.setExitedState(exitedTime + ((float) rnd.NextDouble() - 0.5f) * hysterese * 2.0f);
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
        Destroy(prefab);
    }
}
