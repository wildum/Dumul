using UnityEditor;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

static class CommunicationCenter
{

    private static InfoCanvas infoCanvas1;
    private static InfoCanvas infoCanvas2;
    private static SpectatorsManager spectatorsManager;

    public static void resetCommunicationCenter()
    {
        infoCanvas1 = null;
        infoCanvas2 = null;
        spectatorsManager = null;
    }

    public static void updateHealth()
    {
        if (infoCanvas1 == null || infoCanvas2 == null)
        {
            infoCanvas1 = GameObject.Find("InfoCanvasP1").GetComponent<InfoCanvas>();
            infoCanvas2 = GameObject.Find("InfoCanvasP2").GetComponent<InfoCanvas>();
            spectatorsManager = GameObject.Find("SpectatorsManager").GetComponent<SpectatorsManager>();
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
                if (p.Team == 0)
                {
                    if (p.IdInTeam == 0)
                    {
                        orangeHealth1 = p.getHealth();
                    }
                    else
                    {
                        orangeHealth2 = p.getHealth();
                    }
                }
                else
                {
                    if (p.IdInTeam == 0)
                    {
                        blueHealth1 = p.getHealth();
                    }
                    else
                    {
                        blueHealth2 = p.getHealth();
                    }
                }
            }
            spectatorsManager.currentTeamHealthUpdate(orangeHealth1 + orangeHealth2, blueHealth1 + blueHealth2);
            infoCanvas1.updateHealth(orangeHealth1, orangeHealth2, blueHealth1, blueHealth2);
            infoCanvas2.updateHealth(orangeHealth1, orangeHealth2, blueHealth1, blueHealth2);
        }
        else
        {
            Debug.LogError("cannot set the infocanvas");
        }
    }
}