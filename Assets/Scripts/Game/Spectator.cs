using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spectator : MonoBehaviour
{

    private int team;

    void Start()
    {

    }

    void Update()
    {
        
    }

    public int Team { set { team = value; } get { return team;} }
}
