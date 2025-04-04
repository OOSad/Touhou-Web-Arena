using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class FallingBulletSpawner : NetworkBehaviour
{
    [Header("Spawn Points")]
    [Tooltip("Transform that represents Player 1's bullet spawn area")]
    public Transform player1SpawnPoint;
    
    [Tooltip("Transform that represents Player 2's bullet spawn area")]
    public Transform player2SpawnPoint;
    
    [Header("Spawn Area")]
    [Tooltip("Width of the rectangular spawn area")]
    [Range(1f, 20f)]
    public float spawnRectWidth = 10f;
    
    [Tooltip("Height of the rectangular spawn area")]
    [Range(1f, 10f)]
    public float spawnRectHeight = 3f;
    
    [Header("Boundary Settings")]
    [Tooltip("Tag used for boundary objects that should destroy bullets")]
    public string boundaryTag = "Boundary";
    
    [Header("Bullet Settings")]
    [Tooltip("Small bullet prefab to use for spawning")]
    public GameObject bulletPrefab;
    
    [Tooltip("How many bullets to spawn per second")]
    [Range(1, 50)]
    public float spawnRate = 20f;
    
    [Tooltip("Base speed of bullets")]
    [Range(1f, 10f)]
    public float bulletSpeed = 3f;
    
    [Tooltip("Minimum speed multiplier (1.0 = 100% of base speed)")]
    [Range(0.2f, 1f)]
    public float minSpeedMultiplier = 0.7f;
    
    [Tooltip("Maximum speed multiplier (1.0 = 100% of base speed)")]
    [Range(1f, 3f)]
    public float maxSpeedMultiplier = 1.5f;
    
    [Tooltip("Variation in bullet angles (in degrees)")]
    [Range(5f, 45f)]
    public float angleVariation = 20f;
    
    private float lastSpawnTime;
    
    // Make sure the bullet prefab has a NetworkObject component
    private void Awake()
    {
        if (bulletPrefab != null)
        {
            if (!bulletPrefab.TryGetComponent<NetworkObject>(out _))
            {
                Debug.LogError("Bullet prefab MUST have a NetworkObject component for network synchronization");
            }
            
            if (!bulletPrefab.TryGetComponent<NetworkTransform>(out _))
            {
                Debug.LogError("Bullet prefab should have a NetworkTransform component for proper position sync");
            }
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        Debug.Log("FallingBulletSpawner OnNetworkSpawn - IsServer: " + IsServer);
        
        // Ensure spawn points exist
        if (player1SpawnPoint == null || player2SpawnPoint == null)
        {
            Debug.LogError("Spawn points must be assigned!");
            enabled = false;
            return;
        }
        
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet prefab must be assigned!");
            enabled = false;
            return;
        }
        
        if (IsServer)
        {
            Debug.Log("FallingBulletSpawner is ready to spawn bullets");
        }
    }
    
    void Update()
    {
        // Only the server should handle spawning bullets
        if (!IsServer) return;
        
        if (Time.time - lastSpawnTime >= 1f / spawnRate)
        {
            Debug.Log("Server triggering bullet spawn");
            
            // Server generates all random values for bullet 1
            Vector2 pos1 = GetRandomPositionInRectangle(player1SpawnPoint.position);
            float angle1 = 270f + Random.Range(-angleVariation, angleVariation);
            float speed1 = bulletSpeed * Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
            
            // Server generates all random values for bullet 2
            Vector2 pos2 = GetRandomPositionInRectangle(player2SpawnPoint.position);
            float angle2 = 270f + Random.Range(-angleVariation, angleVariation);
            float speed2 = bulletSpeed * Random.Range(minSpeedMultiplier, maxSpeedMultiplier);
            
            // Server spawns the bullets itself
            SpawnServerBullet(pos1, angle1, speed1, true); // true = from player 1's side
            SpawnServerBullet(pos2, angle2, speed2, false); // false = from player 2's side
            
            lastSpawnTime = Time.time;
        }
    }
    
    // Server spawns a bullet that will be network-synchronized
    void SpawnServerBullet(Vector2 spawnPosition, float angle, float speed, bool isPlayer1Side)
    {
        if (!IsServer) return;
        
        // Instantiate the bullet on the server
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.Euler(0, 0, angle));
        
        // Get the BulletBoundaryChecker (should be on the prefab)
        BulletBoundaryChecker boundaryChecker = bullet.GetComponent<BulletBoundaryChecker>();
        if (boundaryChecker != null)
        {
            boundaryChecker.Initialize(this, isPlayer1Side, boundaryTag);
        }
        else
        {
            Debug.LogWarning("Bullet prefab is missing BulletBoundaryChecker component!");
        }
        
        // Get the NetworkBulletMover (should be on the prefab)
        NetworkBulletMover mover = bullet.GetComponent<NetworkBulletMover>();
        if (mover != null)
        {
            // Set up movement direction and speed
            float radianAngle = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radianAngle), Mathf.Sin(radianAngle));
            mover.Initialize(direction, speed);
        }
        else
        {
            Debug.LogWarning("Bullet prefab is missing NetworkBulletMover component!");
        }
        
        // Make bullet a network object spawned by server
        NetworkObject netObj = bullet.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
            Debug.Log("Server spawned network bullet");
        }
        else
        {
            Debug.LogWarning("Bullet prefab does not have a NetworkObject component!");
        }
        
        // Clean up bullets that go off screen (as a backup)
        Destroy(bullet, 10f);
    }
    
    // This method is no longer used - kept for reference
    [ClientRpc]
    void SpawnSpecificBulletsClientRpc(
        Vector2 position1, float angle1, float speed1,
        Vector2 position2, float angle2, float speed2)
    {
        // This RPC is now unused since we're spawning bullets on the server
        // and letting NetworkObject synchronize them
        Debug.Log("Client RPC received but not used in server-authority model");
    }
    
    // Get a random position inside a rectangle centered at the given point
    Vector2 GetRandomPositionInRectangle(Vector3 centerPoint)
    {
        float halfWidth = spawnRectWidth / 2f;
        float halfHeight = spawnRectHeight / 2f;
        
        float randomX = Random.Range(-halfWidth, halfWidth);
        float randomY = Random.Range(-halfHeight, halfHeight);
        
        return new Vector2(
            centerPoint.x + randomX,
            centerPoint.y + randomY
        );
    }
} 