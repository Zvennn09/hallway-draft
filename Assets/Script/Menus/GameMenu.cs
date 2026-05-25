using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private string GameName = "MAIN GAME";

    public void StartGame()
    {
        SceneManager.LoadScene(GameName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
