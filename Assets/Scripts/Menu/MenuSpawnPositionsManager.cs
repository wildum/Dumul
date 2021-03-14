using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace menu
{
    public class MenuStartPosition
    {
        public Vector3 position;
        public Vector3 rotation;
        public int team;
        public int id;

        public MenuStartPosition()
        {
            position = new Vector3(0,0,0);
            rotation = new Vector3(0,0,0);
            team = 0;
            id = -1;
        }

        public MenuStartPosition(Vector3 pos, Vector3 ori, int iteam, int iid)
        {
            position = pos;
            rotation = ori;
            team = iteam;
            id = iid;
        }
    }

    static class MenuSpawnPositionsManager
    {
        private static List<MenuStartPosition> playersStartPos = new List<MenuStartPosition> {
            new MenuStartPosition(new Vector3(-0.844f, 0, 0), new Vector3(0, 0, 0), 0, 1),
            new MenuStartPosition(new Vector3(0.844f, 0, 0), new Vector3(0, 0, 0), 1, 2),
            new MenuStartPosition(new Vector3(-2.175f, 0, 0), new Vector3(0, 0, 0), 0, 3),
            new MenuStartPosition(new Vector3(2.175f, 0, 0), new Vector3(0, 0, 0), 1, 4)
        };

        public static MenuStartPosition getMenuStartPosition(int id)
        {
            Dictionary<int, Photon.Realtime.Player> dict = PhotonNetwork.CurrentRoom.Players;
            List<int> actorNumbers = new List<int>();
            foreach (KeyValuePair<int, Photon.Realtime.Player> entry in dict)
            {
                actorNumbers.Add(entry.Value.ActorNumber);
            }

            actorNumbers.Sort();

            for (int i = 0; i < actorNumbers.Count; i++)
            {
                if (actorNumbers[i] == id)
                {
                    return playersStartPos[i%playersStartPos.Count];
                }
            }
            Debug.LogError("Actor not found, snh");
            return new MenuStartPosition();
        }
    }
}
