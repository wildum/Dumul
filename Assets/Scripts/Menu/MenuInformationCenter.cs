using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace menu
{
    static class MenuInformationCenter
    {
        private static List<NetworkPlayerMenu> players = new List<NetworkPlayerMenu>();
        public static List<NetworkPlayerMenu> getPlayers()
        {
            updatePlayersList();
            return players;
        }
        private static void updatePlayersList()
        {
            NetworkPlayerMenu[] playersArray = GameObject.FindObjectsOfType<NetworkPlayerMenu>();
            players.Clear();
            foreach (NetworkPlayerMenu p in playersArray)
            {
                players.Add(p);
            }
        }
    }
}