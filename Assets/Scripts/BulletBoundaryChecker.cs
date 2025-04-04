using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class BulletBoundaryChecker : NetworkBehaviour
{
    private string boundaryTagName;
    private float overlapCheckRadius = 0.1f;
    private bool isBeingDestroyed = false;
    
    [Tooltip("Delay in seconds before destroying the bullet after collision")]
    private float despawnDelay = 0.5f;
    
    public void Initialize(FallingBulletSpawner spawnerRef, bool fromPlayer1Side, string boundaryTag)
    {
        boundaryTagName = boundaryTag;
        
        // Make sure bullets have a collider for boundary collision detection
        if (GetComponent<Collider2D>() == null)
        {
            // Add a small circle collider if none exists
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.1f; // Small collision radius
            collider.isTrigger = true; // Use trigger for less physical interference
        }
    }
    
    void Update()
    {
        // Only server can destroy bullets
        if (!IsServer) return;
        
        // Check for tunneling through thin colliders
        CheckForBoundaryOverlap();
    }
    
    // Prevent bullets from tunneling through thin colliders
    private void CheckForBoundaryOverlap()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, overlapCheckRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag(boundaryTagName))
            {
                TriggerDespawnWithDelay();
                return;
            }
        }
    }
    
    // Handle collisions with boundary objects
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only server can destroy bullets
        if (!IsServer) return;
        
        // Check if the collision is with a boundary object
        if (collision.gameObject.CompareTag(boundaryTagName))
        {
            TriggerDespawnWithDelay();
        }
    }
    
    void OnTriggerEnter2D(Collider2D collider)
    {
        // Only server can destroy bullets
        if (!IsServer) return;
        
        // Check if the collision is with a boundary object
        if (collider.CompareTag(boundaryTagName))
        {
            TriggerDespawnWithDelay();
        }
    }
    
    // Start the delayed destruction process
    private void TriggerDespawnWithDelay()
    {
        // Avoid multiple destroy attempts
        if (isBeingDestroyed) return;
        
        isBeingDestroyed = true;
        StartCoroutine(DestroyAfterDelay());
    }
    
    // Coroutine to add a delay before destruction
    private IEnumerator DestroyAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(despawnDelay);
        
        // Now destroy the bullet
        DestroyBullet();
    }
    
    // Centralized method to handle bullet destruction
    private void DestroyBullet()
    {
        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj != null && netObj.IsSpawned)
        {
            netObj.Despawn();
        }
        else
        {
            Destroy(gameObject);
        }
    }
} 