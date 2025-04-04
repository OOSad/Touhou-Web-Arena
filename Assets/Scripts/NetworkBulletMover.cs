using UnityEngine;
using Unity.Netcode;

public class NetworkBulletMover : NetworkBehaviour
{
    private Vector2 direction;
    private float speed;
    private bool initialized = false;
    
    public void Initialize(Vector2 moveDirection, float moveSpeed)
    {
        direction = moveDirection;
        speed = moveSpeed;
        initialized = true;
    }
    
    void Update()
    {
        if (!initialized) return;
        
        // Move the bullet (server authoritative, but runs on all clients)
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
} 