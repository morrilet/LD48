using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariables : MonoBehaviour
{
    public const float GRAVITY = -9.8f;
    public const string FISH_TAG = "Fish";
    public const string TETHER_TAG = "Tether";
    public const string PLAYER_TAG = "Player";
    public const int LEVEL_GEOMETRY_LAYER_ID = 18;
    public const int PLAYER_LAYER_ID = 11;

    // These are used for influencing enemy spawns.
    public const float TIER_0_DEPTH = 20;
    public const float TIER_1_DEPTH = 200;
    public const float TIER_2_DEPTH = 750;

    public const string SFX_PLAYER_HIT = "PlayerHit";
    public const string SFX_PLAYER_JUMP = "PlayerJump";
    public const string SFX_PLAYER_WALL_SLIDE = "PlayerSlide";
    public const string SFX_PLAYER_DIE = "PlayerDie";
    public const string SFX_FISH_WINDUP = "FishWindup";
    public const string SFX_FISH_ATTACK = "FishAttack";

    public const int MAIN_MENU_SCENE_ID = 0;
    public const int GAME_SCENE_ID = 1;
}