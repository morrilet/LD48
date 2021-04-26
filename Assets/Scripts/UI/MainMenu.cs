using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Slider effectSlider;
    [SerializeField] private Slider musicSlider;

    private void Start() {
        effectSlider.value = AudioManager.instance.masterEffectVolume;
        musicSlider.value = AudioManager.instance.masterMusicVolume;
    }

    public void Play() {
        SceneManager.LoadScene(GlobalVariables.GAME_SCENE_ID);
    }

    public void Quit() {
        Application.Quit();
    }

    public void UpdateEffectVolume(float value) {
        AudioManager.instance.masterEffectVolume = value;
    }
    
    public void UpdateMusicVolume(float value) {
        AudioManager.instance.UpdateMusicSourceVolumes(value);
    }
}
