using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private string characterName;

    // Called when the network spawns this object
    public override void OnNetworkSpawn()
    {
        // Only enable input processing if this is the local player
        enabled = IsOwner;
        
        if (enabled)
        {
            Debug.Log($"You are controlling {characterName}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction
        Vector3 movement = new Vector3(horizontal, vertical, 0f) * moveSpeed * Time.deltaTime;

        // Apply movement
        transform.position += movement;
        
        // The NetworkTransform component must be added to the prefab in the Inspector
        // It will automatically synchronize position changes
    }
} 