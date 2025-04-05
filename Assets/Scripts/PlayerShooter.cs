using UnityEngine;
using Unity.Netcode;
// using DanmakU;
using System.Collections;

public class PlayerShooter : NetworkBehaviour
{
    [Header("Bullet Configuration")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletSize = 0.3f;
    [SerializeField] private Color bulletColor = Color.red;
    
    [Header("Firing Pattern")]
    [SerializeField] private int bulletsPerShot = 2;        // Side-by-side bullets (pairs)
    [SerializeField] private float bulletSpread = 0.4f;     // Horizontal distance between bullet pairs
    [SerializeField] private int shotsPerPress = 3;         // Number of shots fired per Z press
    [SerializeField] private float shotDelay = 0.02f;       // Delay between shots in a burst
    
    // Internal variables
    private float nextFireTime;
    private bool isFiring;
    private Coroutine firingCoroutine;
    
    public override void OnNetworkSpawn()
    {
        // Only enable input processing for the local player
        enabled = IsOwner;
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        
        // Check for fire button tap (Z key)
        if (Input.GetKeyDown(KeyCode.Z) && Time.time >= nextFireTime && !isFiring)
        {
            FireBurstServerRpc();
            nextFireTime = Time.time + (shotDelay * shotsPerPress); // Simple cooldown based on burst duration
        }
    }
    
    [ServerRpc]
    private void FireBurstServerRpc()
    {
        // Tell all clients to fire a burst
        FireBurstClientRpc();
    }
    
    [ClientRpc]
    private void FireBurstClientRpc()
    {
        // Cancel any existing firing coroutine
        if (firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
        }
        
        // Start a new firing sequence
        firingCoroutine = StartCoroutine(FireBurstRoutine());
    }
    
    private IEnumerator FireBurstRoutine()
    {
        isFiring = true;
        
        // Fire the specified number of shots
        for (int i = 0; i < shotsPerPress; i++)
        {
            // Fire a bullet pair
            FireBulletPair();
            
            // Wait between shots
            if (i < shotsPerPress - 1) // Don't wait after the last shot
            {
                yield return new WaitForSeconds(shotDelay);
            }
        }
        
        isFiring = false;
        firingCoroutine = null;
    }
    
    private void FireBulletPair()
    {
        if (bulletPrefab == null) return;
        
        // Get firing position (slightly in front of the player)
        Vector2 firePosition = transform.position;
        firePosition.y += 0.5f; // Adjust based on character size
        
        // Direction is always upward in this game style
        Vector2 fireDirection = Vector2.up;
        
        // Spawn bullet pairs
        for (int i = 0; i < bulletsPerShot; i++)
        {
            // Calculate position offset for bullet pairs
            float offset = (i - (bulletsPerShot - 1) / 2f) * bulletSpread;
            Vector2 offsetPosition = firePosition + new Vector2(offset, 0);
            
            // Fire the bullet from the calculated position
            FireBullet(offsetPosition, fireDirection);
        }
    }
    
    private void FireBullet(Vector2 position, Vector2 direction)
    {
        if (bulletPrefab == null) return;
        
        // Instantiate the bullet
        GameObject bulletObj = Instantiate(bulletPrefab, position, Quaternion.identity);
        
        // Get and configure the bullet component
        PlayerBullet bullet = bulletObj.GetComponent<PlayerBullet>();
        if (bullet != null)
        {
            bullet.Initialize(direction, bulletSpeed, bulletColor, bulletSize);
        }
        
        // Set up the network object
        NetworkObject networkObj = bulletObj.GetComponent<NetworkObject>();
        if (networkObj != null && IsServer)
        {
            networkObj.Spawn();
        }
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy();
        
        if (firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
        }
    }
} 