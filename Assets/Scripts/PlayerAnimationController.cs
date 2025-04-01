using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerController))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerController playerController;
    
    // Animation parameter names - can be customized in the Inspector
    [Header("Animation Parameter Names")]
    [SerializeField] private string idleParameterName = "IsIdle";
    [SerializeField] private string moveLeftParameterName = "IsMovingLeft";
    [SerializeField] private string moveRightParameterName = "IsMovingRight";
    
    // Track previous movement direction to detect changes
    private bool wasMovingLeft = false;
    private bool wasMovingRight = false;
    
    [Header("Transition Settings")]
    [SerializeField] private float directionChangeTransitionSpeed = 0.01f;
    
    void Start()
    {
        // Get required components
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }
    
    void Update()
    {
        // Update animator parameters based on player movement state
        if (animator != null)
        {
            // Check for direction changes
            bool directionChanged = (wasMovingLeft && playerController.IsMovingRight) || 
                                   (wasMovingRight && playerController.IsMovingLeft);
            
            if (directionChanged)
            {
                // Force a quick transition by setting transition speed
                animator.speed = 2f;  // Temporarily increase animation speed
                
                // Reset any current transitions to ensure immediate response
                animator.CrossFade(playerController.IsMovingLeft ? 
                    animator.GetCurrentAnimatorStateInfo(0).shortNameHash : 
                    animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 
                    directionChangeTransitionSpeed);
            }
            else
            {
                // Normal animation speed
                animator.speed = 1f;
            }
            
            // Set animation states based on movement
            animator.SetBool(moveLeftParameterName, playerController.IsMovingLeft);
            animator.SetBool(moveRightParameterName, playerController.IsMovingRight);
            animator.SetBool(idleParameterName, !playerController.IsMoving);
            
            // Update tracking variables for next frame
            wasMovingLeft = playerController.IsMovingLeft;
            wasMovingRight = playerController.IsMovingRight;
        }
    }
}