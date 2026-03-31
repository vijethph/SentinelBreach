using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls CIPHER's movement using entirely custom physics math.
/// NO Rigidbody.AddForce, NO Physics.gravity — all motion is computed manually.
/// Uses CharacterController ONLY for collision geometry detection (isGrounded, Move).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR-EXPOSED SETTINGS
    // ─────────────────────────────────────────────

    [Header("Movement")]
    [Tooltip("Forward run speed in units/second. Increases with SENTINEL tier later.")]
    public float runSpeed = 8f;

    [Tooltip("X position of each lane: left=-2.5, centre=0, right=+2.5")]
    public float laneWidth = 2.5f;

    [Tooltip("Speed at which CIPHER lerps between lane positions.")]
    public float laneSwitchSpeed = 10f;

    [Header("Custom Gravity & Jump — Student-Written Physics")]
    [Tooltip("Custom gravity constant. Negative = downward. NOT using Physics.gravity.")]
    public float gravity = -25f;

    [Tooltip("Initial upward velocity applied on jump. NOT using AddForce.")]
    public float jumpForce = 12f;

    [Header("Slide")]
    [Tooltip("CharacterController height during a slide.")]
    public float slideHeight = 0.5f;

    [Tooltip("CharacterController height when standing.")]
    public float normalHeight = 1.8f;

    [Tooltip("How long a slide lasts in seconds.")]
    public float slideDuration = 0.8f;

    [Header("Custom Knockback — Student-Written Physics")]
    [Tooltip("How fast knockback velocity decays per second (exponential decay).")]
    public float knockbackDecay = 5f;

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────

    private CharacterController cc;

    // Custom physics state — no Rigidbody
    private float verticalVelocity = 0f;
    private Vector3 knockbackVelocity = Vector3.zero;

    // Lane state
    private int currentLane = 1;      // 0=left, 1=centre, 2=right
    private float targetX = 0f;       // target world X position

    // Slide state
    private bool isSliding = false;
    private float slideTimer = 0f;

    // Public state for other systems
    public float DistanceRun { get; private set; } = 0f;
    public bool IsRunning { get; private set; } = true;
    public bool IsGrounded => cc.isGrounded;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
    {
        cc = GetComponent<CharacterController>();

        // Initialise lane to centre
        currentLane = 1;
        targetX = 0f;

        // Set CharacterController to standing height
        cc.height = normalHeight;
        cc.center = new Vector3(0f, normalHeight / 2f, 0f);
    }

    void Update()
    {
        if (!IsRunning) return;

        HandleInput();
        HandleSlide();
        ApplyMovement();
        TrackDistance();
    }

    // ─────────────────────────────────────────────
    // INPUT HANDLING (legacy Input — refactored Week 4)
    // ─────────────────────────────────────────────

    void HandleInput()
    {
        // Lane switch LEFT
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (currentLane > 0)
            {
                currentLane--;
                targetX = (currentLane - 1) * laneWidth;
            }
        }

        // Lane switch RIGHT
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (currentLane < 2)
            {
                currentLane++;
                targetX = (currentLane - 1) * laneWidth;
            }
        }

        // Jump — only when grounded
        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded)
        {
            // CUSTOM PHYSICS: direct velocity assignment — NOT AddForce
            verticalVelocity = jumpForce;
        }

        // Slide — only when grounded and not already sliding
		// LeftShift is used instead of LeftControl — Ctrl is caught by OS/editor on some platforms
		if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.S))
			&& cc.isGrounded && !isSliding)
		{
			StartSlide();
		}
    }

    // ─────────────────────────────────────────────
    // SLIDE
    // ─────────────────────────────────────────────

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;

        // Shrink the CharacterController so CIPHER fits under obstacles
        cc.height = slideHeight;
        cc.center = new Vector3(0f, slideHeight / 2f, 0f);
    }

    void HandleSlide()
    {
        if (!isSliding) return;

        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0f)
        {
            isSliding = false;
            cc.height = normalHeight;
            cc.center = new Vector3(0f, normalHeight / 2f, 0f);
        }
    }

    // ─────────────────────────────────────────────
    // MOVEMENT — ALL CUSTOM PHYSICS
    // ─────────────────────────────────────────────

    void ApplyMovement()
    {
        // ── 1. Forward movement (constant auto-run) ──────────────────
        Vector3 move = Vector3.forward * runSpeed;

        // ── 2. Lateral lane positioning (Lerp-based smoothing) ────────
        // We compute how far we need to move laterally this frame
        // and inject it as a velocity rather than snapping.
        float currentX = transform.position.x;
        float newX = Mathf.Lerp(currentX, targetX, laneSwitchSpeed * Time.deltaTime);
        // Convert position delta to velocity component
        move.x = (newX - currentX) / Time.deltaTime;

        // ── 3. CUSTOM GRAVITY ─────────────────────────────────────────
        // Accumulate gravity manually — equivalent to: v = v₀ + a·t
        // This is student-written physics, NOT Physics.gravity
        if (!cc.isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        else if (verticalVelocity < 0f)
        {
            // Small negative snap when grounded prevents accumulation
            verticalVelocity = -2f;
        }

        move.y = verticalVelocity;

        // ── 4. CUSTOM KNOCKBACK DECAY ─────────────────────────────────
        // Exponential decay: velocity approaches zero asymptotically
        // This is student-written physics, NOT a physics material or damping
        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );
        move += knockbackVelocity;

        // ── 5. Apply via CharacterController ─────────────────────────
        // CharacterController.Move handles collision geometry only —
        // it does NOT apply any physics forces itself.
        cc.Move(move * Time.deltaTime);
    }

    // ─────────────────────────────────────────────
    // DISTANCE TRACKING
    // ─────────────────────────────────────────────

    void TrackDistance()
    {
        // Distance = speed × time (simple integration)
        DistanceRun += runSpeed * Time.deltaTime;
    }

    // ─────────────────────────────────────────────
    // PUBLIC METHODS — called by other systems
    // ─────────────────────────────────────────────

    /// <summary>
    /// Applies a knockback impulse away from the damage source.
    /// CUSTOM PHYSICS — computed from direction vector, NOT AddForce.
    /// </summary>
    /// <param name="sourcePosition">World position of the thing that hit the player.</param>
    /// <param name="force">Knockback magnitude. Default 8.</param>
    public void ApplyKnockback(Vector3 sourcePosition, float force = 8f)
    {
        // Compute direction from source to player
        Vector3 dir = (transform.position - sourcePosition).normalized;

        // Add a slight upward component so knockback has visible lift
        dir.y = 0.4f;
        dir.Normalize();

        // Set knockback velocity — will decay via exponential decay in ApplyMovement
        // This is student-written physics: impulse = direction × magnitude
        knockbackVelocity = dir * force;
    }

    /// <summary>
    /// Stops all movement. Called on death.
    /// </summary>
    public void StopRunning()
    {
        IsRunning = false;
        knockbackVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    /// <summary>
    /// Increases run speed. Called by Skill Tree upgrades.
    /// </summary>
    public void SetRunSpeed(float newSpeed)
    {
        runSpeed = newSpeed;
    }
}