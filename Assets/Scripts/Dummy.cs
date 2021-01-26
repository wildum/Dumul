using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour
{

    public void takeDamage(int damageAmount, int playerId)
    {
        Debug.Log("Dummy took " + damageAmount + " damages from player " + playerId);
    }

}
