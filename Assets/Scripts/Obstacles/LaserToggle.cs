using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class LaserToggle : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("How long the laser stays ON (active/dangerous).")]
    public float onDuration = 1.5f;

    [Tooltip("How long the laser stays OFF (safe window to pass).")]
    public float offDuration = 1.0f;

    [Tooltip("Random offset added to start time so multiple lasers don't all sync.")]
    public float randomStartOffset = 0f;

    [Header("References")]
    [Tooltip("The Line Renderer component that renders the beam.")]
    public LineRenderer laserBeam;

    [Tooltip("The Box Collider (Is Trigger) on this object.")]
    public Collider laserCollider;

    void Start()
    {
        
        randomStartOffset = Random.Range(0f, onDuration + offDuration);
        StartCoroutine(ToggleLaser());
    }

    IEnumerator ToggleLaser()
    {
        
        yield return new WaitForSeconds(randomStartOffset);

        while (true)
        {
            
            SetLaserActive(true);
            yield return new WaitForSeconds(onDuration);

            
            SetLaserActive(false);
            yield return new WaitForSeconds(offDuration);
        }
    }

    void SetLaserActive(bool active)
    {
        if (laserBeam != null)   laserBeam.enabled = active;
        if (laserCollider != null) laserCollider.enabled = active;
    }

    
    
    
    public void ForceOff(float duration)
    {
        StartCoroutine(ForceOffCoroutine(duration));
    }

    IEnumerator ForceOffCoroutine(float duration)
    {
        StopCoroutine(nameof(ToggleLaser));
        SetLaserActive(false);
        yield return new WaitForSeconds(duration);
        StartCoroutine(ToggleLaser());
    }
}