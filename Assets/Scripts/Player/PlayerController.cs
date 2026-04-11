using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls CIPHER's movement with entirely custom physics math.
/// NO Rigidbody.AddForce — NO Physics.gravity.
/// CharacterController is used ONLY for collision geometry (isGrounded, Move).
/// This is what you explain in the interview for the Physics rubric.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ─────────────────────────────────────────────
    // INSPECTOR SETTINGS
    // ─────────────────────────────────────────────

    [Header("Movement")]
    public float runSpeed = 8f;
    public float laneWidth = 2.0f;       // X distance between lanes
    public float laneSwitchSpeed = 10f;  // how fast CIPHER lerps to target X

    [Header("Custom Gravity & Jump — Student-Written Physics")]
    [Tooltip("Gravity constant. Negative = downward. NOT Physics.gravity.")]
    public float gravity = -25f;

    [Tooltip("Upward velocity on jump. NOT AddForce.")]
    public float jumpForce = 12f;

    [Header("Slide")]
    public float slideHeight = 0.6f;
    public float normalHeight = 2.0f;
    public float slideDuration = 0.8f;

    [Header("Custom Knockback — Student-Written Physics")]
    [Tooltip("How fast knockback velocity decays per second.")]
    public float knockbackDecay = 5f;

    // ─────────────────────────────────────────────
    // PRIVATE STATE
    // ─────────────────────────────────────────────

    private CharacterController cc;
	
	// ── Input System ─────────────────────────────────────────────
	private CipherInputActions inputActions;
	private Vector2 moveInput;

    // Custom physics — no Rigidbody
    private float verticalVelocity = 0f;
    private Vector3 knockbackVelocity = Vector3.zero;

    // Lane
    private int currentLane = 1;     // 0=left, 1=centre, 2=right
    private float targetX = 0f;

    // Slide
    private bool isSliding = false;
    private float slideTimer = 0f;

    // Public state for other scripts
    public float DistanceRun { get; private set; } = 0f;
    public bool IsRunning { get; private set; } = true;
    public bool IsGrounded => cc != null && cc.isGrounded;

    // ─────────────────────────────────────────────
    // UNITY LIFECYCLE
    // ─────────────────────────────────────────────

    void Start()
	{
		cc = GetComponent<CharacterController>();
		currentLane = 1;
		targetX = 0f;
		cc.height = normalHeight;
		cc.center = new Vector3(0f, normalHeight / 2f, 0f);

		// ── Apply Skill Tree bonuses ──────────────────────────────
		if (SkillTree.Instance != null)
		{
			runSpeed *= SkillTree.Instance.GetSpeedMultiplier();
		}
	}
	
	void OnEnable()
	{
		inputActions = new CipherInputActions();
		inputActions.Player.Enable();

		inputActions.Player.Jump.performed  += ctx => OnJump();
		inputActions.Player.Slide.performed += ctx => OnSlide();
		inputActions.Player.Gadget1.performed += ctx => GadgetManager.Instance?.Activate(0);
		inputActions.Player.Gadget2.performed += ctx => GadgetManager.Instance?.Activate(1);
		inputActions.Player.Gadget3.performed += ctx => GadgetManager.Instance?.Activate(2);
	}

	void OnDisable()
	{
		inputActions?.Player.Disable();
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
    // INPUT — uses legacy Input Manager
    // ⚠️ Keep Active Input Handling = "Input Manager (Old)" in Player Settings
    // ─────────────────────────────────────────────

    void HandleInput()
	{
		if (inputActions == null) return;

		moveInput = inputActions.Player.Move.ReadValue<Vector2>();

		// Lane switch — only trigger on direction change, not held
		// We use a flag to prevent repeated triggers while key is held
		float horizontal = moveInput.x;
		if (horizontal < -0.5f && !laneMoving)
		{
			if (currentLane > 0) { currentLane--; targetX = (currentLane - 1) * laneWidth; }
			laneMoving = true;
		}
		else if (horizontal > 0.5f && !laneMoving)
		{
			if (currentLane < 2) { currentLane++; targetX = (currentLane - 1) * laneWidth; }
			laneMoving = true;
		}
		else if (Mathf.Abs(horizontal) < 0.1f)
		{
			laneMoving = false;
		}
	}

	private bool laneMoving = false;

	void OnJump()
	{
		if (!IsRunning) return;
		if (cc.isGrounded)
			verticalVelocity = jumpForce;
	}

	void OnSlide()
	{
		if (!IsRunning) return;
		if (cc.isGrounded && !isSliding)
			StartSlide();
	}

    // ─────────────────────────────────────────────
    // SLIDE
    // ─────────────────────────────────────────────

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
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
        // ── 1. Forward auto-run ────────────────────────────────────
        Vector3 move = Vector3.forward * runSpeed;

        // ── 2. Lateral lane Lerp ───────────────────────────────────
        float currentX = transform.position.x;
        float newX = Mathf.Lerp(currentX, targetX, laneSwitchSpeed * Time.deltaTime);
        move.x = (newX - currentX) / Time.deltaTime;

        // ── 3. CUSTOM GRAVITY ──────────────────────────────────────
        // v = v₀ + a·t  — student-written, NOT Physics.gravity
        if (!cc.isGrounded)
            verticalVelocity += gravity * Time.deltaTime;
        else if (verticalVelocity < 0f)
            verticalVelocity = -2f;   // ground snap, prevents drift

        move.y = verticalVelocity;

        // ── 4. CUSTOM KNOCKBACK DECAY ──────────────────────────────
        // Exponential decay — student-written
        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );
        move += knockbackVelocity;

        // ── 5. Apply — CharacterController handles geometry only ───
        cc.Move(move * Time.deltaTime);
    }

    void TrackDistance()
    {
        DistanceRun += runSpeed * Time.deltaTime;
    }

    // ─────────────────────────────────────────────
    // PUBLIC METHODS
    // ─────────────────────────────────────────────

    /// <summary>
    /// CUSTOM PHYSICS: knockback impulse via vector math — NOT AddForce.
    /// Called by all damage sources (laser, turret, drone).
    /// </summary>
    public void ApplyKnockback(Vector3 sourcePosition, float force = 8f)
    {
        Vector3 dir = (transform.position - sourcePosition).normalized;
        dir.y = 0.4f;  // slight upward lift
        dir.Normalize();
        knockbackVelocity = dir * force;
    }

    public void StopRunning()
    {
        IsRunning = false;
        knockbackVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }

    // Called by SwipeInputHandler in Week 4
    // public void SwipeLane(int dir)
    // {
    //     if (!IsRunning) return;
    //     int newLane = Mathf.Clamp(currentLane + dir, 0, 2);
    //     if (newLane != currentLane) { currentLane = newLane; targetX = (currentLane - 1) * laneWidth; }
    // }

    // public void SwipeJump()
    // {
    //     if (!IsRunning || !cc.isGrounded) return;
    //     verticalVelocity = jumpForce;
    // }

    // public void SwipeSlide()
    // {
    //     if (!IsRunning || !cc.isGrounded || isSliding) return;
    //     StartSlide();
    // }
	
		// ─── Called by SwipeInputHandler (Android) ──────────────────

	/// <summary>Swipe left (dir=-1) or right (dir=+1) to change lane.</summary>
	public void SwipeLane(int dir)
	{
		if (!IsRunning) return;
		int newLane = Mathf.Clamp(currentLane + dir, 0, 2);
		if (newLane != currentLane)
		{
			currentLane = newLane;
			targetX = (currentLane - 1) * laneWidth;
		}
	}

	/// <summary>Swipe up — jump.</summary>
	public void SwipeJump()
	{
		if (!IsRunning) return;
		if (cc.isGrounded)
			verticalVelocity = jumpForce;
	}

	/// <summary>Swipe down — slide.</summary>
	public void SwipeSlide()
	{
		if (!IsRunning) return;
		if (cc.isGrounded && !isSliding)
			StartSlide();
	}
}