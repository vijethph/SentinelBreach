using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float runSpeed = 8f;
    public float laneWidth = 2.0f;
    public float laneSwitchSpeed = 10f;
	private float gravityEffective;

    [Header("Custom Gravity & Jump")]
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

    private CharacterController cc;

	private CipherInputActions inputActions;
	private Vector2 moveInput;

    private float verticalVelocity = 0f;
    private Vector3 knockbackVelocity = Vector3.zero;

    private int currentLane = 1;
    private float targetX = 0f;

    private bool isSliding = false;
    private float slideTimer = 0f;
	private bool isJumping = false;

	[Header("Slide Physics (student-written)")]
	public float slideSpeedBoost = 3f;
	public float slideDrag       = 4f;
	private float slideExtraSpeed = 0f;
	
	[Header("Slide Resize Smoothing")]
	public float slideResizeDuration = 0.10f;   // seconds to complete capsule resize
	private Coroutine slideResizeCoroutine;
	
	[Header("Corridor Bounds")]
	[Tooltip("World Y of the ceiling face. Must match the segment prefab ceiling height.")]
	public float corridorCeiling = 3.85f; 

    public float DistanceRun { get; private set; } = 0f;
    public bool IsRunning { get; private set; } = true;
    public bool IsGrounded => cc != null && cc.isGrounded;

    void Start()
	{
		cc = GetComponent<CharacterController>();
		currentLane = 1;
		targetX = 0f;
		gravityEffective = gravity;   
		
		cc.height = normalHeight;
		cc.center = new Vector3(0f, normalHeight / 2f, 0f);

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
	
	IEnumerator SmoothJumpCoroutine()
	{
		// Frame 1: apply 55% of jump force — feels more like a physical push-off
		verticalVelocity = jumpForce * 0.55f;
		yield return null;
		// Frame 2: full force — two-frame ramp removes the instantaneous velocity pop
		verticalVelocity = jumpForce;
	}

    void HandleInput()
	{
		if (inputActions == null) return;

		moveInput = inputActions.Player.Move.ReadValue<Vector2>();

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
		if (cc.isGrounded && !isJumping)
		{
			isJumping = true;
			StartCoroutine(SmoothJumpCoroutine());
		}
	}

	void OnSlide()
	{
		if (!IsRunning) return;
		if (cc.isGrounded && !isSliding)
			StartSlide();
	}
	
	IEnumerator ResizeCapsule(float fromHeight, float toHeight, float duration)
	{
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			// Smoothstep ease: t² (3 - 2t)
			float tEased = t * t * (3f - 2f * t);
			float h = Mathf.Lerp(fromHeight, toHeight, tEased);
			cc.height = h;
			cc.center = new Vector3(0f, h / 2f, 0f);
			yield return null;
		}
		cc.height = toHeight;
		cc.center = new Vector3(0f, toHeight / 2f, 0f);
	}

    void StartSlide()
    {
        isSliding = true;
		slideExtraSpeed = slideSpeedBoost;
        slideTimer = slideDuration;
        if (slideResizeCoroutine != null) StopCoroutine(slideResizeCoroutine);
		slideResizeCoroutine = StartCoroutine(ResizeCapsule(normalHeight, slideHeight, slideResizeDuration));
    }

    void HandleSlide()
    {
        if (!isSliding) return;
        slideTimer -= Time.deltaTime;
        if (slideTimer <= 0f)
        {
            isSliding = false;
			if (slideResizeCoroutine != null) StopCoroutine(slideResizeCoroutine);
			slideResizeCoroutine = StartCoroutine(ResizeCapsule(slideHeight, normalHeight, slideResizeDuration));
        }
    }

    void ApplyMovement()
    {
        Vector3 move = Vector3.forward * runSpeed;

		if (isSliding && slideExtraSpeed > 0f)
		{
			slideExtraSpeed = Mathf.Lerp(slideExtraSpeed, 0f, slideDrag * Time.deltaTime);
			if (slideExtraSpeed < 0.05f) slideExtraSpeed = 0f;
		}

		float effectiveSpeed = runSpeed + slideExtraSpeed;

        float currentX = transform.position.x;
        float newX = Mathf.Lerp(currentX, targetX, laneSwitchSpeed * Time.deltaTime);
        move.x = (newX - currentX) / Time.deltaTime;

        if (!cc.isGrounded)
		{
			gravityEffective = (GravityInversion.Instance != null && GravityInversion.Instance.IsInverted)
				? Mathf.Abs(gravity)     // positive → accelerates upward
				: gravity;               // negative → accelerates downward (normal)

			verticalVelocity += gravityEffective * Time.deltaTime;
			// verticalVelocity += gravity * Time.deltaTime;
		}
            
        else if (verticalVelocity < 0f)
		{
			// Smooth landing: Lerp toward the resting value rather than hard-clamping
			verticalVelocity = Mathf.Lerp(verticalVelocity, -2f, 18f * Time.deltaTime);
			if (Mathf.Abs(verticalVelocity - (-2f)) < 0.1f)
				verticalVelocity = -2f;
			isJumping = false;   // ← reset jump flag on grounded
		}

        move.y = verticalVelocity;

        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );
        move += knockbackVelocity;

        cc.Move(move * Time.deltaTime);

		move.z = effectiveSpeed;
		
		// ── Inverted grounding: clamp CIPHER to ceiling when inverted ────
		if (GravityInversion.Instance != null && GravityInversion.Instance.IsInverted)
		{
			// Corridor ceiling is at Y = corridorHeight (default 4.0).
			// When inverted, treat ceiling contact as "grounded".
			// CharacterController handles this via its collider — no manual clamp needed
			// as long as the ceiling has a MeshCollider or plain Cube collider.
			// Reset vertical velocity on ceiling contact (mirrors the floor grounded reset).
			if (transform.position.y >= corridorCeiling - 0.05f)
			{
				verticalVelocity = 0f;
			}
		}
    }

    void TrackDistance()
    {
        DistanceRun += runSpeed * Time.deltaTime;
    }

    public void ApplyKnockback(Vector3 sourcePosition, float force = 8f)
    {
        Vector3 dir = (transform.position - sourcePosition).normalized;
		dir.y = 0.4f;
        dir.Normalize();
        knockbackVelocity = dir * force;
    }

    public void StopRunning()
    {
        IsRunning = false;
        knockbackVelocity = Vector3.zero;
        verticalVelocity = 0f;
    }



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


	public void SwipeJump()
	{
		if (!IsRunning) return;
		if (cc.isGrounded && !isJumping)
		{
			isJumping = true;
			StartCoroutine(SmoothJumpCoroutine());
		}
	}


	public void SwipeSlide()
	{
		if (!IsRunning) return;
		if (cc.isGrounded && !isSliding)
			StartSlide();
	}
	
	/// <summary>
	/// Called by GravityInversion to flip the effective gravity direction.
	/// Also resets verticalVelocity to give CIPHER an initial push toward the new surface.
	/// </summary>
	public void SetGravityInverted(bool inverted)
	{
		// Give a small initial impulse so CIPHER starts moving toward the new surface
		// rather than waiting for gravity alone to accelerate it.
		verticalVelocity = inverted ? Mathf.Abs(gravity) * 0.3f : gravity * 0.3f;
		Debug.Log($"[PlayerController] Gravity inverted: {inverted}. Initial vel: {verticalVelocity:F2}");
	}
}