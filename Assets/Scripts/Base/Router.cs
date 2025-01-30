using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Router : MonoBehaviour
{
    [System.Serializable]
    public class Player
    {
        public PlayerMovement movement;
        public PlayerCombat combat;
        public PlayerStatus status;
        public PlayerClimb climb;
    }

    [System.Serializable]
    public class Camera
    {
        public CameraFollow follow;
        public CameraZoom zoom;
    }
    
    [System.Serializable]
    public class Main
    {
        public GameController gameController;
    }

    public Main main;
    public Player player;
    public Camera camera;

    void Awake()
    {
        main.gameController = FindFirstObjectByType<GameController>();
        
        player.movement = FindFirstObjectByType<PlayerMovement>();
        player.combat = FindFirstObjectByType<PlayerCombat>();
        player.status = FindFirstObjectByType<PlayerStatus>();
        player.climb = FindFirstObjectByType<PlayerClimb>();
        
        camera.follow = FindFirstObjectByType<CameraFollow>();
        camera.zoom = FindFirstObjectByType<CameraZoom>();
    }
}
