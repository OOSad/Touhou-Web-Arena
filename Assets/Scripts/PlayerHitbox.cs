using UnityEngine;
using Unity.Netcode;

public class PlayerHitbox : NetworkBehaviour
{
    [SerializeField] private GameObject hitboxPrefab;
    
    private PlayerController playerController;
    private GameObject hitboxInstance;
    private SpriteRenderer hitboxRenderer;
    private bool isVisible = false;
    
    // Cache reference to the PlayerController
    void Start()
    {
        playerController = GetComponent<PlayerController>();
        
        if (hitboxPrefab == null)
        {
            Debug.LogError("Hitbox prefab not assigned to PlayerHitbox!");
            return;
        }
        
        // Instantiate hitbox - the hitbox is always active, only its visibility changes
        hitboxInstance = Instantiate(hitboxPrefab, transform.position, Quaternion.identity);
        hitboxInstance.transform.SetParent(transform);
        hitboxInstance.transform.localPosition = Vector3.zero; // Center on character
        
        // Get reference to the renderer component
        hitboxRenderer = hitboxInstance.GetComponent<SpriteRenderer>();
        if (hitboxRenderer == null)
        {
            Debug.LogError("Hitbox prefab must have a SpriteRenderer component!");
            return;
        }
        
        // Make the hitbox invisible initially but keep it active
        hitboxRenderer.enabled = false;
    }
    
    void Update()
    {
        // Only process input for the local player
        if (!IsOwner) return;
        
        bool shiftPressed = Input.GetKey(KeyCode.LeftShift);
        
        // Toggle hitbox visibility based on shift key state
        if (shiftPressed && !isVisible)
        {
            ShowHitboxVisual();
        }
        else if (!shiftPressed && isVisible)
        {
            HideHitboxVisual();
        }
    }
    
    private void ShowHitboxVisual()
    {
        if (hitboxRenderer != null)
        {
            hitboxRenderer.enabled = true;
            isVisible = true;
        }
    }
    
    private void HideHitboxVisual()
    {
        if (hitboxRenderer != null)
        {
            hitboxRenderer.enabled = false;
            isVisible = false;
        }
    }
    
    // Clean up when destroyed
    void OnDestroy()
    {
        if (hitboxInstance != null)
        {
            Destroy(hitboxInstance);
        }
    }
} 