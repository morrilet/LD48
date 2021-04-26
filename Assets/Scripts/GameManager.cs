using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [HideInInspector] public GameObject player {get; private set;}
    [HideInInspector] public GameObject tether {get; private set;}
    [HideInInspector] public float playerDepth {get; private set;}

    float startYPosition;

    private void Awake() {
        if (instance != null)
            GameObject.DestroyImmediate(this.gameObject);
        instance = this;

        player = GameObject.FindGameObjectWithTag(GlobalVariables.PLAYER_TAG);
        tether = GameObject.FindGameObjectWithTag(GlobalVariables.TETHER_TAG);
        
        startYPosition = player.transform.position.y;
    }

    private void Update() {
        playerDepth = Mathf.Abs(startYPosition - player.transform.position.y);
    }

    // public struct DifficultyTier {
    //     public int index;
    //     public string name;
    //     public float depth;
    // }
}
