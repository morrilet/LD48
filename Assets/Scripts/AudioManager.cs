using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public float masterMusicVolume = 0.75f;
    public float masterEffectVolume = 1.0f;
    public AudioClipData[] audioClips;

    AudioSource effectSource;

    private void Awake() {
        if (instance != null) {
            GameObject.DestroyImmediate(this.gameObject);
            return;
        }
        instance = this;

        GameObject.DontDestroyOnLoad(this);

        effectSource = gameObject.AddComponent<AudioSource>();
        effectSource.playOnAwake = false;
        effectSource.loop = false;

        for (int i = 0; i < audioClips.Length; i++) {
            if (audioClips[i].isMusic) {
                audioClips[i].source = gameObject.AddComponent<AudioSource>();
                audioClips[i].source.volume = masterMusicVolume;
                audioClips[i].source.clip = audioClips[i].clip;
                audioClips[i].source.loop = true;
                audioClips[i].source.Play();
            }

            if (audioClips[i].isLoopingEffect) {
                audioClips[i].source = gameObject.AddComponent<AudioSource>();
                audioClips[i].source.volume = masterEffectVolume;
                audioClips[i].source.clip = audioClips[i].clip;
                audioClips[i].source.playOnAwake = false;
                audioClips[i].source.loop = true;
            }
        }

        Debug.Log(effectSource);
    }

    private void Update() {
        float depth = GameManager.instance ? GameManager.instance.playerDepth : 0;
        UpdateDepthClips(depth);
    }

    public void UpdateMusicSourceVolumes(float volume) {
        masterMusicVolume = volume;
        for (int i = 0; i < audioClips.Length; i++) {
            if (audioClips[i].isMusic)
                audioClips[i].source.volume = masterMusicVolume;
        }
    }

    public void PlayEffect(string effectName) {
        AudioClipData effect = audioClips.Where(i => !i.isMusic && i.name == effectName).FirstOrDefault();
        if (effect.clip != null) {  // Default will just be empty AudioClipData - check for that.

            if (effect.isLoopingEffect && !effect.source.isPlaying) {
                effect.source.volume = masterEffectVolume;
                effect.source.Play();
            } else {
                effectSource.volume = masterEffectVolume;
                effectSource.PlayOneShot(effect.clip);
            }
        }
    }

    public void StopEffect(string effectName) {
        AudioClipData effect = audioClips.Where(i => !i.isMusic && i.name == effectName).FirstOrDefault();
        if (effect.clip != null) {  // Default will just be empty AudioClipData - check for that.

            if (effect.isLoopingEffect && effect.source.isPlaying) {
                effect.source.Stop();
            }
        }
    }

    private void UpdateDepthClips(float depth) {
        AudioClipData[] depthClipsEnable = audioClips.Where(data => data.isMusic && data.useDepthEnable).ToArray();
        for (int i = 0; i < depthClipsEnable.Length; i++) {
            float adjustedDepthStart = depth - depthClipsEnable[i].depthEnabledStart;
            float adjustedDepthEnd = depthClipsEnable[i].depthEnabledEnd - depthClipsEnable[i].depthEnabledStart;
            float percent = Mathf.Clamp01(adjustedDepthStart / adjustedDepthEnd);

            depthClipsEnable[i].source.volume = percent * masterMusicVolume;
        }

        // AudioClipData[] depthClipsDisable = audioClips.Where(data => data.isMusic && data.useDepthDisable).ToArray();
        // for (int i = 0; i < depthClipsDisable.Length; i++) {
        //     float adjustedDepthStart = GameManager.instance.playerDepth - depthClipsEnable[i].depthDisableStart;
        //     float adjustedDepthEnd = depthClipsEnable[i].depthDisableEnd - depthClipsEnable[i].depthDisableStart;
        //     float percent = Mathf.Clamp01(adjustedDepthStart / adjustedDepthEnd);

        //     depthClipsDisable[i].source.volume = percent * masterMusicVolume;
        // }
    }

    [System.Serializable]
    public struct AudioClipData {
        public string name;
        public AudioClip clip;
        [HideInInspector] public AudioSource source;
        public bool isLoopingEffect;
        public bool isMusic;
        public bool useDepthEnable;
        public float depthEnabledStart;  // What depth to start fading in the audio.
        public float depthEnabledEnd;  // What depth to stop fading in the audio.
        // public bool useDepthDisable;
        // public float depthDisableStart;  // What depth to start fading out the audio.
        // public float depthDisableEnd;  // What depth to stop fading out the audio.
    }
}
