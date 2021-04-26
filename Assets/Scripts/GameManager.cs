using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    [SerializeField] float deathMenuDelay = 1.25f;
    [SerializeField] GameObject deathMenu;
    [SerializeField] GameObject hud;

    [HideInInspector] public GameObject player {get; private set;}
    [HideInInspector] public GameObject tether {get; private set;}
    [HideInInspector] public float playerDepth {get; private set;}
    [HideInInspector] public float rawPlayerDepth {get; private set;}

    float startYPosition;
    bool trackDepth = true;

    private void Awake() {
        if (instance != null)
            GameObject.DestroyImmediate(this.gameObject);
        instance = this;

        player = GameObject.FindGameObjectWithTag(GlobalVariables.PLAYER_TAG);
        tether = GameObject.FindGameObjectWithTag(GlobalVariables.TETHER_TAG);
        
        startYPosition = player.transform.position.y;

        deathMenu.SetActive(false);
        hud.SetActive(true);
    }

    private void Update() {
        rawPlayerDepth = Mathf.Abs(startYPosition - player.transform.position.y);
        if (trackDepth)
            playerDepth = rawPlayerDepth;
    }

    public void KillPlayer() {
        StartCoroutine(KillPlayerCoroutine());
    }

    private IEnumerator KillPlayerCoroutine() {
        player.GetComponent<CharacterController_Platformer>().Die();
        trackDepth = false;

        yield return new WaitForSeconds(deathMenuDelay);
        
        deathMenu.SetActive(true);
        hud.SetActive(false);
    }

    public void StunPlayer(Vector3 sourcePosition, float duration) {
        player.GetComponent<CharacterController_Platformer>().Stun(sourcePosition, duration);
    }

    public string FormattedDepth() {
        return System.String.Format("{0:n0}", playerDepth);
    }

    // public struct DifficultyTier {
    //     public int index;
    //     public string name;
    //     public float depth;
    // }
}
