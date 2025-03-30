using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private string characterName;
    
    // Flag to track if boundaries have been properly set
    private bool boundariesSet = false;
    
    // Current active boundaries - initialized with wide values to prevent premature clamping
    private float leftBoundary = -100f;
    private float rightBoundary = 100f;
    private float topBoundary = 100f;
    private float bottomBoundary = -100f;
    
    [Header("Boundary Configuration")]
    // Vertical boundaries (shared by both players)
    [Tooltip("Top boundary for both players")]
    [SerializeField] private float sharedTopBoundary = 4f;
    [Tooltip("Bottom boundary for both players")]
    [SerializeField] private float sharedBottomBoundary = -4f;
    
    [Header("Player 1 Boundaries (Left Side)")]
    [SerializeField] private float p1LeftBoundary = -8f;
    [SerializeField] private float p1RightBoundary = -1f;
    
    [Header("Player 2 Boundaries (Right Side)")]
    [SerializeField] private float p2LeftBoundary = 1f;
    [SerializeField] private float p2RightBoundary = 8f;
    
    // Cached player number from PlayerSpawner
    private int playerNumber = 0;

    // Called when the network spawns this object
    public override void OnNetworkSpawn()
    {
        // Only enable input processing if this is the local player
        enabled = IsOwner;
        
        if (enabled)
        {
            Debug.Log($"You are controlling {characterName}");
            
            // Get player number after a short delay to ensure PlayerSpawner has assigned it
            StartCoroutine(GetPlayerNumberFromNetworkManager());
        }
    }
    
    private IEnumerator GetPlayerNumberFromNetworkManager()
    {
        // Wait a moment to ensure PlayerSpawner has time to assign player numbers
        yield return new WaitForSeconds(0.5f);
        
        // Get client ID
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        
        // Get PlayerSpawner reference using non-deprecated method with correct parameter order
        PlayerSpawner playerSpawner = Object.FindFirstObjectByType<PlayerSpawner>(FindObjectsInactive.Exclude);
        if (playerSpawner != null)
        {
            // Request player number from server
            playerSpawner.GetPlayerNumberServerRpc(clientId);
        }
        else
        {
            Debug.LogError("Could not find PlayerSpawner in the scene!");
        }
    }
    
    // This will be called by the PlayerSpawner to set our player number
    public void SetPlayerNumber(int number)
    {
        playerNumber = number;
        Debug.Log($"PlayerController received player number: {playerNumber}");
        
        // Set boundaries based on player number
        SetBoundariesForPlayerNumber(playerNumber);
    }
    
    // Set the appropriate boundaries based on player number
    private void SetBoundariesForPlayerNumber(int playerNum)
    {
        // Set shared vertical boundaries
        topBoundary = sharedTopBoundary;
        bottomBoundary = sharedBottomBoundary;
        
        // Set horizontal boundaries based on player number
        if (playerNum == 1)
        {
            // Player 1 (left side)
            leftBoundary = p1LeftBoundary;
            rightBoundary = p1RightBoundary;
            Debug.Log($"Setting boundaries for Player 1 (left side): {leftBoundary} to {rightBoundary}, vertical: {bottomBoundary} to {topBoundary}");
        }
        else if (playerNum == 2)
        {
            // Player 2 (right side)
            leftBoundary = p2LeftBoundary;
            rightBoundary = p2RightBoundary;
            Debug.Log($"Setting boundaries for Player 2 (right side): {leftBoundary} to {rightBoundary}, vertical: {bottomBoundary} to {topBoundary}");
        }
        else
        {
            Debug.LogWarning($"Unknown player number: {playerNum}, using default boundaries");
        }
        
        // Mark boundaries as properly set
        boundariesSet = true;
        
        // Log the spawn position for debugging
        Debug.Log($"Player {playerNum} spawned at position: {transform.position}");
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        // Get raw digital input (no acceleration/deceleration)
        float horizontal = 0f;
        float vertical = 0f;
        
        // Horizontal movement (left/right)
        if (Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;
            
        // Vertical movement (up/down)
        if (Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;
        else if (Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;
            
        // Add support for diagonal movement at normal speed
        if (horizontal != 0f && vertical != 0f)
        {
            // Normalize diagonal movement to prevent faster diagonal speed
            float diagonalFactor = 0.7071f; // approximately 1/√2
            horizontal *= diagonalFactor;
            vertical *= diagonalFactor;
        }

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontal, vertical, 0f) * moveSpeed * Time.deltaTime;

        // Apply movement
        transform.position += movement;
    }
    
    // LateUpdate runs after all Update methods - perfect for enforcing boundaries
    void LateUpdate()
    {
        // Skip boundary enforcement if we're not the owner or boundaries aren't set yet
        if (!IsOwner || !boundariesSet) return;
        
        // Enforce play field boundaries
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, leftBoundary, rightBoundary);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, bottomBoundary, topBoundary);
        transform.position = clampedPosition;
        
        // The NetworkTransform component will automatically synchronize the position
    }
} 