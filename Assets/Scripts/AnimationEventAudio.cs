using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventAudio : MonoBehaviour
{
    public void PlayEffect(string name) {
        AudioManager.instance.PlayEffect(name);
    }
}
