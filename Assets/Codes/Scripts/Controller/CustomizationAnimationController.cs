using UnityEngine;

/// <summary>
/// Manages animation state transitions for a character customization system.
/// Controls switching between idle and walking animations based on timed intervals.
/// 
/// Features:
/// - Timed state machine for alternating idle and walking animations
/// - Configurable durations for idle and walking states
/// - Simple integration with Unity's Animator component
/// </summary>
public class CustomizationAnimationController : MonoBehaviour
{
    [Header("Animation Settings")]
    public Animator animator;
    public float idleDuration = 5f;
    public float walkDuration = 5f;

    private float timer = 0f;
    private bool isWalking = false;

    void Start()
    {
        if (animator == null)
        {
            Debug.LogError("Animator not assigned on CustomizationAnimationController.");
        }

        animator.SetBool("IsWalking", false);
        // Start with idle state
        timer = idleDuration;
    }

    void Update()
    {
        if (animator == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            isWalking = !isWalking;
            animator.SetBool("IsWalking", isWalking);
            // Reset timer based on new state
            timer = isWalking ? walkDuration : idleDuration;
        }
    }
}