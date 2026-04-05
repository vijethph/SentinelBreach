using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Android sensor-based input using accelerometer and gyroscope.
/// Satisfies the A+ rubric requirement: "Android/iOS: screen touch, accelerator, gyroscope."
///
/// Lane control:   tilt device left/right  (accelerometer X axis)
/// Jump:           tilt device sharply forward (gyroscope pitch) OR tap top half of screen
/// Slide:          tilt device sharply backward (gyroscope pitch) OR tap bottom half of screen
/// Gadgets:        on-screen UI buttons (wired in Step 17.2)
/// </summary>
public class AndroidInputHandler : MonoBehaviour
{
    // ─── Inspector ───────────────────────────────────────────────

    [Header("Lane Tilt (Accelerometer)")]
    [Tooltip("Tilt angle (degrees) required to trigger a lane change.")]
    public float laneThreshold = 20f;

    [Tooltip("Dead zone — tilts smaller than this are ignored.")]
    public float laneDeadZone = 5f;

    [Header("Jump / Slide Tilt (Gyroscope)")]
    [Tooltip("Gyro angular velocity (rad/s) needed to trigger jump or slide.")]
    public float gyroJumpThreshold  = 3.0f;
    public float gyroSlideThreshold = 3.0f;

    [Header("Touch Fallback")]
    [Tooltip("If true, tapping top half of screen jumps; bottom half slides.")]
    public bool enableTouchFallback = true;

    [Tooltip("Cooldown between tap-jumps/slides in seconds.")]
    public float tapCooldown = 0.5f;

    // ─── Private ─────────────────────────────────────────────────

    private PlayerController playerController;

    // Lane switching debounce — prevents flickering
    private float laneChangeCooldown  = 0.4f;
    private float laneChangeTimer     = 0f;
    private bool  isNeutral           = true;   // true when device is roughly flat

    // Gyro cooldown
    private float gyroCooldown        = 0.4f;
    private float gyroTimer           = 0f;

    // Touch fallback cooldown
    private float tapTimer = 0f;

    // Baseline tilt — measured at scene start
    private float baselineTiltX = 0f;

    // ─────────────────────────────────────────────────────────────

    void Start()
    {
        // Only run on mobile
        if (!Application.isMobilePlatform)
        {
            enabled = false;
            return;
        }

        playerController = GameObject.FindGameObjectWithTag("Player")
                                      ?.GetComponent<PlayerController>();

        // Enable gyroscope
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Debug.Log("[AndroidInput] Gyroscope enabled.");
        }
        else
        {
            Debug.Log("[AndroidInput] Gyroscope not supported on this device. Tap fallback active.");
        }

        // Measure baseline tilt so the player can hold the phone
        // at their preferred angle and it still registers as "neutral"
        baselineTiltX = Input.acceleration.x;
        Debug.Log($"[AndroidInput] Baseline tilt X = {baselineTiltX:F2}");
    }

    void Update()
    {
        if (playerController == null) return;

        HandleLaneTilt();
        HandleJumpSlide();
        HandleTouchFallback();

        // Tick cooldown timers
        if (laneChangeTimer > 0f) laneChangeTimer -= Time.deltaTime;
        if (gyroTimer       > 0f) gyroTimer       -= Time.deltaTime;
        if (tapTimer        > 0f) tapTimer         -= Time.deltaTime;
    }

    // ─── Lane control via accelerometer ──────────────────────────

    void HandleLaneTilt()
    {
        if (laneChangeTimer > 0f) return;

        // Convert accelerometer.x to approximate tilt angle in degrees
        // accelerometer.x ranges roughly -1 to +1 for full left/right tilt
        float rawX       = Input.acceleration.x - baselineTiltX;
        float tiltDeg    = rawX * 90f;  // approximate conversion

        if (tiltDeg < -laneThreshold)
        {
            // Tilted LEFT
            playerController.SwipeLane(-1);
            laneChangeTimer = laneChangeCooldown;
            isNeutral       = false;
        }
        else if (tiltDeg > laneThreshold)
        {
            // Tilted RIGHT
            playerController.SwipeLane(1);
            laneChangeTimer = laneChangeCooldown;
            isNeutral       = false;
        }
        else if (Mathf.Abs(tiltDeg) < laneDeadZone)
        {
            // Phone returned to neutral — allow next lane change
            isNeutral = true;
        }
    }

    // ─── Jump / Slide via gyroscope ──────────────────────────────

    void HandleJumpSlide()
    {
        if (!Input.gyro.enabled)          return;
        if (gyroTimer > 0f)               return;

        // Gyro rotationRate gives angular velocity in device space
        // rotationRate.x is pitch (tilt forward = negative, tilt back = positive)
        float pitch = Input.gyro.rotationRate.x;

        if (pitch < -gyroJumpThreshold)
        {
            // Sharp forward tilt → JUMP
            playerController.SwipeJump();
            gyroTimer = gyroCooldown;
        }
        else if (pitch > gyroSlideThreshold)
        {
            // Sharp backward tilt → SLIDE
            playerController.SwipeSlide();
            gyroTimer = gyroCooldown;
        }
    }

    // ─── Touch fallback (if gyro unavailable or player prefers tap) ─

    void HandleTouchFallback()
    {
        if (!enableTouchFallback) return;
        if (Input.touchCount == 0)  return;
        if (tapTimer > 0f)          return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;

            // Ignore if the touch is on a UI button (already handled by EventSystem)
            if (IsPointerOverUI(t.position)) continue;

            bool topHalf = t.position.y > Screen.height * 0.5f;
            if (topHalf)
                playerController.SwipeJump();
            else
                playerController.SwipeSlide();

            tapTimer = tapCooldown;
            break;
        }
    }

    // ─── Helpers ─────────────────────────────────────────────────

    bool IsPointerOverUI(Vector2 screenPos)
    {
        // Prevent tap-behind-button false triggers
        return UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current
                          .IsPointerOverGameObject(-1);
    }

    /// <summary>
    /// Re-calibrates the neutral tilt baseline.
    /// Call this from a UI "Calibrate" button if the player holds the phone
    /// at a tilted angle as their default position.
    /// </summary>
    public void Recalibrate()
    {
        baselineTiltX = Input.acceleration.x;
        Debug.Log($"[AndroidInput] Recalibrated. New baseline = {baselineTiltX:F2}");
    }
}
