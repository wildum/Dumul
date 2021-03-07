using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{

    public void loadOneVsOne()
    {
        Debug.Log("load scene one v one");
        SceneManager.LoadScene("Arena");
    }

    public void quitGame()
    {
        Application.Quit();
    }
}