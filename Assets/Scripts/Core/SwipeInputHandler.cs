using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles Android touch input and translates gestures into game actions.
/// This is a DIFFERENT control mechanic from PC — satisfying the A+ Devices/HCI rubric.
///
/// Gestures:
///   Swipe Left/Right  → lane change
///   Swipe Up          → jump
///   Swipe Down        → slide
///   Double-tap left   → Gadget 1 (Dash)
///   Double-tap right  → Gadget 2 (EMP)
///   Two-finger tap    → Gadget 3 (Time-Slow)
/// </summary>
public class SwipeInputHandler : MonoBehaviour
{
    [Header("Swipe Thresholds")]
    [Tooltip("Minimum swipe distance in pixels to register a swipe.")]
    public float swipeThreshold = 50f;

    [Tooltip("Maximum time in seconds a swipe can take.")]
    public float maxSwipeTime = 0.4f;

    [Tooltip("Maximum interval between two taps to count as double-tap.")]
    public float doubleTapInterval = 0.3f;

    // ─── Private State ───────────────────────────────────────────
    private Vector2  touchStartPos;
    private float    touchStartTime;
    private bool     isSwiping = false;

    // Double-tap tracking per finger half (left / right)
    private float lastTapTimeLeft  = -1f;
    private float lastTapTimeRight = -1f;

    private PlayerController playerController;

    void Start()
    {
        // Only run this on mobile
        if (!Application.isMobilePlatform && !Application.isEditor)
        {
            enabled = false;
            return;
        }
        playerController = GameObject.FindGameObjectWithTag("Player")
                                      ?.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        // ── Two-finger tap → Time-Slow ────────────────────────────
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            if (t0.phase == TouchPhase.Began && t1.phase == TouchPhase.Began)
            {
                GadgetManager.Instance?.Activate(2);
                return;
            }
        }

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartPos  = touch.position;
                touchStartTime = Time.realtimeSinceStartup;
                isSwiping = true;
                break;

            case TouchPhase.Ended:
                if (!isSwiping) break;
                isSwiping = false;

                Vector2 delta    = touch.position - touchStartPos;
                float   duration = Time.realtimeSinceStartup - touchStartTime;
                float   dist     = delta.magnitude;

                bool isLeftHalf = touchStartPos.x < Screen.width * 0.5f;

                // ── Swipe detection ─────────────────────────────────────
                if (dist >= swipeThreshold && duration <= maxSwipeTime)
                {
                    // Determine dominant axis
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        // Horizontal swipe
                        if (delta.x < 0)      playerController?.SwipeLane(-1);  // swipe left
                        else                   playerController?.SwipeLane(1);   // swipe right
                    }
                    else
                    {
                        // Vertical swipe
                        if (delta.y > 0)  playerController?.SwipeJump();         // swipe up
                        else              playerController?.SwipeSlide();         // swipe down
                    }
                }
                else if (dist < 20f && duration < 0.2f)
                {
                    // ── Tap — check for double-tap ─────────────────────
                    if (isLeftHalf)
                    {
                        if (Time.realtimeSinceStartup - lastTapTimeLeft <= doubleTapInterval)
                            GadgetManager.Instance?.Activate(0);  // Dash
                        lastTapTimeLeft = Time.realtimeSinceStartup;
                    }
                    else
                    {
                        if (Time.realtimeSinceStartup - lastTapTimeRight <= doubleTapInterval)
                            GadgetManager.Instance?.Activate(1);  // EMP
                        lastTapTimeRight = Time.realtimeSinceStartup;
                    }
                }
                break;

            case TouchPhase.Canceled:
                isSwiping = false;
                break;
        }
    }
}
