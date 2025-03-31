using UnityEngine;
using Unity.Netcode;

public class EnemyController : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float moveSpeed = 2f;
    
    // Use NetworkVariable to sync health across clients
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Initialize health on the server
            currentHealth.Value = maxHealth;
        }
    }
    
    private void Update()
    {
        if (!IsServer) return;
        
        // Simple movement pattern
        // You can implement more complex enemy movement here
        MoveEnemy();
    }
    
    private void MoveEnemy()
    {
        // Example: Move in a simple pattern
        // Can be expanded for more complex movement
        float xMovement = Mathf.Sin(Time.time) * moveSpeed * Time.deltaTime;
        transform.position += new Vector3(xMovement, -moveSpeed * 0.5f * Time.deltaTime, 0);
        
        // Destroy if it moves off the bottom of the screen
        if (transform.position.y < -10)
        {
            NetworkObject.Despawn();
        }
    }
    
    // Called by the bullet when it hits the enemy
    public void TakeDamage(int damage)
    {
        if (!IsServer) return;
        
        // Only the server can apply damage
        TakeDamageServerRpc(damage);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage)
    {
        currentHealth.Value -= damage;
        
        // Check if enemy is defeated
        if (currentHealth.Value <= 0)
        {
            // Enemy defeated
            DestroyServerRpc();
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DestroyServerRpc()
    {
        // Could spawn effects, add score, etc.
        
        // Despawn the network object
        NetworkObject.Despawn();
    }
} 