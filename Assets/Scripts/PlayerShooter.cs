using UnityEngine;
using Unity.Netcode;
using DanmakU;
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
    [SerializeField] private float shotDelay = 0.05f;       // Delay between shots in a burst
    
    [Header("Rapid Fire Detection")]
    [SerializeField] private float rapidFireThreshold = 0.3f;   // Time window to detect rapid presses
    [SerializeField] private float continuousShotDelay = 0.06f; // Delay between shots during continuous fire
    
    // Internal variables
    private float nextFireTime;
    private float lastPressTime;
    private bool isFiring;
    private bool isRapidFiring;
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
        if (Input.GetKeyDown(KeyCode.Z))
        {
            float timeSinceLastPress = Time.time - lastPressTime;
            lastPressTime = Time.time;
            
            // If currently firing, and the press was rapid, switch to continuous mode
            if (isFiring && timeSinceLastPress < rapidFireThreshold)
            {
                isRapidFiring = true;
                // No need to start a new coroutine, the existing one will adjust
            }
            // Otherwise start a new firing sequence if not in cooldown
            else if (Time.time >= nextFireTime && !isFiring)
            {
                isRapidFiring = false;
                FireBurstServerRpc();
                nextFireTime = Time.time + rapidFireThreshold; // Smaller cooldown to allow rapid fire detection
            }
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
        int shotsFired = 0;
        
        while (true)
        {
            // Fire a bullet pair
            FireBulletPair();
            shotsFired++;
            
            // Determine which delay to use based on firing mode
            float currentDelay = isRapidFiring ? continuousShotDelay : shotDelay;
            
            // Wait the appropriate delay
            yield return new WaitForSeconds(currentDelay);
            
            // If not in rapid fire mode and we've fired enough shots, exit
            if (!isRapidFiring && shotsFired >= shotsPerPress)
            {
                break;
            }
            
            // If in rapid fire mode, check if we should continue
            if (isRapidFiring && Time.time - lastPressTime > rapidFireThreshold)
            {
                // Player hasn't pressed Z recently, end the continuous stream
                isRapidFiring = false;
                
                // If we've already fired at least the minimum shots, exit
                if (shotsFired >= shotsPerPress)
                {
                    break;
                }
                // Otherwise, continue until minimum shots are fired
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