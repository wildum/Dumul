using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorsManager : MonoBehaviour
{

    public GameObject prefab;
    public Material matBlue;
    public Material matOrange;

    private Spectator[] spectators = new Spectator[210];
    private float[] zPos = {-12.131f, -12.616f, -13.1f, -13.5f, -13.87f, -13.987f, -14.115f, -14.184f, -14.184f, -14.184f, -14.184f,
                            -14.184f, -14.184f, -14.184f, -14.115f, -13.987f, -13.87f, -13.5f, -13.1f, -12.616f, -12.131f};
    private int specPerLine = 21;
    private int specPerColumn = 5;
    private int sides = 2;

    private static System.Random rnd = new System.Random();

    void Start()
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
                    int index = i*specPerColumn*sides + j*sides + side;
                    spectators[index] = obj.GetComponent<Spectator>();
                    int team = rnd.NextDouble() < 0.5 ? 0 : 1;
                    spectators[index].Team = team;
                    obj.GetComponentInChildren<Renderer>().material = team == 0 ? matOrange : matBlue;
                }
            }
        }
        Destroy(prefab);
    }

    void Update()
    {
        
    }
}
