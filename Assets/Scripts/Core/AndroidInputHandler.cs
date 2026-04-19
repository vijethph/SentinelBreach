using System.Collections;
using System.Collections.Generic;
using UnityEngine;
















public class AndroidInputHandler : MonoBehaviour
{
    

    [Header("Swipe Settings")]
    public float swipeThreshold    = 50f;
    public float maxSwipeTime      = 0.4f;
    public float doubleTapInterval = 0.3f;

    [Header("Tilt Settings (Accelerometer)")]
    public float laneThreshold  = 20f;
    public float laneDeadZone   = 5f;
    public float laneChangeCooldown = 0.4f;

    [Header("Gyroscope Settings")]
    public float gyroJumpThreshold  = 3.0f;
    public float gyroSlideThreshold = 3.0f;
    public float gyroCooldown       = 0.4f;

    [Header("Touch Fallback")]
    public bool  enableTouchFallback = true;
    public float tapCooldown         = 0.5f;

    

    private PlayerController playerController;

    
    private Vector2 touchStartPos;
    private float   touchStartTime;
    private bool    isSwiping;
    private float   lastTapTimeLeft  = -1f;
    private float   lastTapTimeRight = -1f;

    
    private float laneChangeTimer = 0f;
    private float gyroTimer       = 0f;
    private float tapTimer        = 0f;
    private float baselineTiltX   = 0f;
    private bool  isNeutral       = true;

    

    void Start()
    {
        if (!Application.isMobilePlatform)
        {
            enabled = false;
            return;
        }

        playerController = GameObject.FindGameObjectWithTag("Player")
                                     ?.GetComponent<PlayerController>();

        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
            Debug.Log("[AndroidInput] Gyroscope enabled.");
        }
        else
        {
            Debug.Log("[AndroidInput] Gyroscope not available — using touch fallback.");
        }

        baselineTiltX = Input.acceleration.x;
    }

    void Update()
    {
        if (playerController == null) return;

        
        if (laneChangeTimer > 0f) laneChangeTimer -= Time.deltaTime;
        if (gyroTimer       > 0f) gyroTimer       -= Time.deltaTime;
        if (tapTimer        > 0f) tapTimer         -= Time.deltaTime;

        
        HandleSwipeInput();
        HandleSensorInput();
    }

    

    void HandleSwipeInput()
    {
        if (Input.touchCount == 0) return;

        
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
                isSwiping      = true;
                break;

            case TouchPhase.Ended:
                if (!isSwiping) break;
                isSwiping = false;

                Vector2 delta    = touch.position - touchStartPos;
                float   duration = Time.realtimeSinceStartup - touchStartTime;
                float   dist     = delta.magnitude;
                bool    leftHalf = touchStartPos.x < Screen.width * 0.5f;

                if (dist >= swipeThreshold && duration <= maxSwipeTime)
                {
                    
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        
                        playerController.SwipeLane(delta.x < 0f ? -1 : 1);
                    }
                    else
                    {
                        
                        if (delta.y > 0f) playerController.SwipeJump();
                        else              playerController.SwipeSlide();
                    }
                }
                else if (dist < 20f && duration < 0.2f)
                {
                    
                    if (IsPointerOverUI(touch.position)) break;

                    if (leftHalf)
                    {
                        if (Time.realtimeSinceStartup - lastTapTimeLeft <= doubleTapInterval)
                            GadgetManager.Instance?.Activate(0);  
                        lastTapTimeLeft = Time.realtimeSinceStartup;
                    }
                    else
                    {
                        if (Time.realtimeSinceStartup - lastTapTimeRight <= doubleTapInterval)
                            GadgetManager.Instance?.Activate(1);  
                        lastTapTimeRight = Time.realtimeSinceStartup;
                    }
                }
                break;

            case TouchPhase.Canceled:
                isSwiping = false;
                break;
        }
    }

    

    void HandleSensorInput()
    {
        HandleTiltLanes();
        HandleGyroJumpSlide();
        if (enableTouchFallback) HandleTouchFallback();
    }

    void HandleTiltLanes()
    {
        if (laneChangeTimer > 0f) return;

        float rawX   = Input.acceleration.x - baselineTiltX;
        float tiltDeg = rawX * 90f;

        if (tiltDeg < -laneThreshold)
        {
            playerController.SwipeLane(-1);
            laneChangeTimer = laneChangeCooldown;
            isNeutral = false;
        }
        else if (tiltDeg > laneThreshold)
        {
            playerController.SwipeLane(1);
            laneChangeTimer = laneChangeCooldown;
            isNeutral = false;
        }
        else if (Mathf.Abs(tiltDeg) < laneDeadZone)
        {
            isNeutral = true;
        }
    }

    void HandleGyroJumpSlide()
    {
        if (!Input.gyro.enabled || gyroTimer > 0f) return;

        float pitch = Input.gyro.rotationRate.x;

        if (pitch < -gyroJumpThreshold)
        {
            playerController.SwipeJump();
            gyroTimer = gyroCooldown;
        }
        else if (pitch > gyroSlideThreshold)
        {
            playerController.SwipeSlide();
            gyroTimer = gyroCooldown;
        }
    }

    void HandleTouchFallback()
    {
        if (Input.touchCount == 0 || tapTimer > 0f) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != TouchPhase.Began) continue;
            if (IsPointerOverUI(t.position)) continue;

            
            if (!Input.gyro.enabled)
            {
                bool topHalf = t.position.y > Screen.height * 0.5f;
                if (topHalf) playerController.SwipeJump();
                else         playerController.SwipeSlide();
                tapTimer = tapCooldown;
                break;
            }
        }
    }

    

    bool IsPointerOverUI(Vector2 screenPos)
    {
        return UnityEngine.EventSystems.EventSystem.current != null
            && UnityEngine.EventSystems.EventSystem.current
                          .IsPointerOverGameObject(-1);
    }

    public void Recalibrate()
    {
        baselineTiltX = Input.acceleration.x;
        Debug.Log($"[AndroidInput] Recalibrated. Baseline = {baselineTiltX:F2}");
    }
}