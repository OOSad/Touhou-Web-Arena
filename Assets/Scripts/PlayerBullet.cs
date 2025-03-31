using UnityEngine;
using Unity.Netcode;

public class PlayerBullet : NetworkBehaviour
{
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private string enemyTag = "Enemy";
    [SerializeField] private float speed = 10f;
    
    private float creationTime;
    private Vector2 direction = Vector2.up;
    
    // Called to initialize the bullet
    public void Initialize(Vector2 moveDirection, float moveSpeed, Color bulletColor, float bulletSize)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        
        // Set rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Set color if there's a sprite renderer
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = bulletColor;
        }
        
        // Set size of the bullet
        transform.localScale = new Vector3(bulletSize, bulletSize, 1f);
        
        creationTime = Time.time;
    }
    
    private void OnEnable()
    {
        creationTime = Time.time;
    }
    
    private void Update()
    {
        // Move the bullet
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        // Check if bullet has exceeded its lifetime
        if (Time.time > creationTime + lifetime)
        {
            DestroySelf();
        }
        
        // Destroy if gone far off screen
        if (transform.position.magnitude > 30f)
        {
            DestroySelf();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the bullet hit an enemy
        if (other.CompareTag(enemyTag))
        {
            // Handle enemy hit
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(1); // Example damage amount
            }
            
            // Destroy the bullet after hitting
            DestroySelf();
        }
    }
    
    private void DestroySelf()
    {
        if (IsServer)
        {
            // If we're the server, properly despawn the network object
            NetworkObject.Despawn();
        }
        else
        {
            // If we're a client, just destroy locally and the server will handle cleanup
            Destroy(gameObject);
        }
    }
} 