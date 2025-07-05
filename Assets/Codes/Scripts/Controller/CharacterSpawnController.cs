using UnityEngine;

/// <summary>
/// AI-driven character movement and animation system.
/// Provides autonomous behavior for spawned characters with configurable parameters.
/// 
/// Features:
/// - State machine for idle/walking behaviors
/// - Random movement within defined boundaries
/// - Smooth rotation and movement
/// - Configurable timing and speed parameters
/// - Visual debugging with Gizmos
/// </summary>
public class CharacterSpawnController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float walkDuration = 3f;
    [SerializeField] float idleDuration = 2f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] private Animator animator;

    [Header("Movement Area")]
    [SerializeField] float moveRadius = 10f;

    [Header("Status")]
    [SerializeField] bool IsWalking = false;

    // Movement state
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float stateTimer;
    private enum State { Walking, Idle }
    private State currentState;

    void Start()
    {
        startPosition = transform.position;
        currentState = State.Idle;
        // Random initial delay to prevent synchronized movement
        stateTimer = Random.Range(0.5f, idleDuration);
    }
    
    void Update()
    {
        HandleStateMachine();

        if (currentState == State.Walking)
        {
            MoveToTarget();
        }
    }

    // <summary>
    /// Handles the state machine logic for idle and walking states.
    /// Updates animation parameters and transitions between states.
    /// </summary>
    void HandleStateMachine()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.Idle:
                IsWalking = false;
                if (animator != null)
                    animator.SetBool("IsWalking", false);

                // Transition to walking when timer expires
                if (stateTimer <= 0)
                {
                    StartWalking();
                }
                break;

            case State.Walking:
                IsWalking = true;
                if (animator != null)
                    animator.SetBool("IsWalking", true);
                
                // Transition to idle when timer expires or target reached
                if (stateTimer <= 0 || Vector3.Distance(transform.position, targetPosition) < 0.5f)
                {
                    StartIdling();
                }
                break;
        }
    }

    // <summary>
    /// Initiates walking state with random target selection.
    /// Generates a random point within the movement radius.
    /// </summary>
    void StartWalking()
    {
        currentState = State.Walking;
        IsWalking = true;

        // Generate random target position within radius
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(2f, moveRadius);

        targetPosition = startPosition + new Vector3(randomDirection.x * randomDistance, 0, randomDirection.y * randomDistance);

        // Set random walk duration
        stateTimer = Random.Range(walkDuration * 0.5f, walkDuration * 1.5f);

        // Rotate towards target
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Initiates idle state with random duration.
    /// Provides natural variation in idle timing.
    /// </summary>
    void StartIdling()
    {
        currentState = State.Idle;
        IsWalking = false;

        // Set random idle duration for natural behavior
        stateTimer = Random.Range(idleDuration * 0.5f, idleDuration * 1.5f);
    }

    /// <summary>
    /// Handles movement towards target position with smooth rotation.
    /// Updates both position and rotation for natural movement.
    /// </summary>
    void MoveToTarget()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Move towards target
        transform.position += direction * walkSpeed * Time.deltaTime;

        // Rotate towards movement direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Gizmos for visual debugging in Scene view.
    /// Shows movement radius and current target when selected.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // Draw movement radius
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireSphere(center, moveRadius);

        // Draw target position when walking
        if (Application.isPlaying && currentState == State.Walking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(targetPosition, 0.5f);
            Gizmos.DrawLine(transform.position, targetPosition);
        }
    }
}