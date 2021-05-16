using UnityEditor;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

static class CommunicationCenter
{

    private static InfoCanvas infoCanvas1;
    private static InfoCanvas infoCanvas2;

    public static void resetCommunicationCenter()
    {
        infoCanvas1 = null;
        infoCanvas2 = null;
    }

    public static void updateHealth()
    {
        if (infoCanvas1 == null || infoCanvas2 == null)
        {
            infoCanvas1 = GameObject.Find("InfoCanvasP1").GetComponent<InfoCanvas>();
            infoCanvas2 = GameObject.Find("InfoCanvasP2").GetComponent<InfoCanvas>();
        }

        if (infoCanvas1 != null && infoCanvas2 != null)
        {
            List<ArenaPlayer> players = InformationCenter.getPlayers();

            int orangeHealth1 = 0;
            int orangeHealth2 = 0;
            int blueHealth1 = 0;
            int blueHealth2 = 0;

            foreach (ArenaPlayer p in players)
            {
                switch (p.Id)
                {
                    case 0:
                        orangeHealth1 = p.getHealth();
                        break;
                    case 1:
                        if (GameSettings.currentState == State.OneVsOne)
                        {
                            blueHealth1 = p.getHealth();
                        }
                        else
                        {
                            orangeHealth2 = p.getHealth();
                        }
                        break;
                    case 2:
                        blueHealth1 = p.getHealth();
                        break;
                    case 3:
                        blueHealth2 = p.getHealth();
                        break;
                }
                
            }
            infoCanvas1.updateHealth(orangeHealth1, orangeHealth2, blueHealth1, blueHealth2);
            infoCanvas2.updateHealth(orangeHealth1, orangeHealth2, blueHealth1, blueHealth2);
        }
        else
        {
            Debug.LogError("cannot set the infocanvas");
        }
    }
}