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

    private CharacterController cc;

	private CipherInputActions inputActions;
	private Vector2 moveInput;

    private float verticalVelocity = 0f;
    private Vector3 knockbackVelocity = Vector3.zero;

    private int currentLane = 1;
    private float targetX = 0f;

    private bool isSliding = false;
    private float slideTimer = 0f;

	[Header("Slide Physics (student-written)")]
	public float slideSpeedBoost = 3f;
	public float slideDrag       = 4f;
	private float slideExtraSpeed = 0f;

    public float DistanceRun { get; private set; } = 0f;
    public bool IsRunning { get; private set; } = true;
    public bool IsGrounded => cc != null && cc.isGrounded;

    void Start()
	{
		cc = GetComponent<CharacterController>();
		currentLane = 1;
		targetX = 0f;
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
		if (cc.isGrounded)
			verticalVelocity = jumpForce;
	}

	void OnSlide()
	{
		if (!IsRunning) return;
		if (cc.isGrounded && !isSliding)
			StartSlide();
	}

    void StartSlide()
    {
        isSliding = true;
		slideExtraSpeed = slideSpeedBoost;
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
            verticalVelocity += gravity * Time.deltaTime;
        else if (verticalVelocity < 0f)
			verticalVelocity = -2f;

        move.y = verticalVelocity;

        knockbackVelocity = Vector3.Lerp(
            knockbackVelocity,
            Vector3.zero,
            knockbackDecay * Time.deltaTime
        );
        move += knockbackVelocity;

        cc.Move(move * Time.deltaTime);

		move.z = effectiveSpeed;
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
		if (cc.isGrounded)
			verticalVelocity = jumpForce;
	}


	public void SwipeSlide()
	{
		if (!IsRunning) return;
		if (cc.isGrounded && !isSliding)
			StartSlide();
	}
}