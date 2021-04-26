using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class InGameMenu : MonoBehaviour
{
    public void Retry() {
        SceneManager.LoadScene(GlobalVariables.GAME_SCENE_ID);
    }

    public void Quit() {
        SceneManager.LoadScene(GlobalVariables.MAIN_MENU_SCENE_ID);
    }
}
