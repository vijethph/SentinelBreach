using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toggles the laser beam on and off rhythmically.
/// Deals damage when the player runs through an active beam.
/// </summary>
[RequireComponent(typeof(LineRenderer), typeof(BoxCollider))]
public class LaserGridController : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 25;

    [Header("Timing")]
    public float onDuration  = 1.5f;
    public float offDuration = 1.0f;
    public float startOffset = 0f;   // randomise in Inspector per laser

    private LineRenderer lr;
    private BoxCollider  bc;
    private bool isOn = true;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        bc = GetComponent<BoxCollider>();

        // Wire line renderer positions from child emitter transforms
        Transform left  = transform.Find("Emitter_Left");
        Transform right = transform.Find("Emitter_Right");
        if (left != null && right != null)
        {
            lr.positionCount = 2;
            lr.SetPosition(0, left.localPosition);
            lr.SetPosition(1, right.localPosition);
        }
    }

    void OnEnable()
    {
        StartCoroutine(ToggleRoutine());
    }

    IEnumerator ToggleRoutine()
    {
        if (startOffset > 0f)
            yield return new WaitForSeconds(startOffset);

        while (true)
        {
            SetBeam(true);
            yield return new WaitForSeconds(onDuration);
            SetBeam(false);
            yield return new WaitForSeconds(offDuration);
        }
    }

    void SetBeam(bool active)
    {
        isOn = active;
        lr.enabled = active;
        bc.enabled = active;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isOn) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        ph?.TakeDamage(damage, transform.position);
    }

    /// <summary>Called by EMP gadget (Week 3) to force beam off.</summary>
    public void ForceOff(float duration) => StartCoroutine(ForceOffCoroutine(duration));

    IEnumerator ForceOffCoroutine(float dur)
    {
        StopCoroutine(nameof(ToggleRoutine));
        SetBeam(false);
        yield return new WaitForSeconds(dur);
        StartCoroutine(ToggleRoutine());
    }
}