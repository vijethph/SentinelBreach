using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Identifies a collectible type and handles pickup when the player enters its trigger.
/// </summary>
public enum CollectibleType { DataShard, ShieldCell, SurgeToken, GhostChip }

public class Collectible : MonoBehaviour
{
    [Tooltip("What type of collectible this is.")]
    public CollectibleType collectibleType;

    [Tooltip("Rotation speed in degrees/second for visual spin effect.")]
    public float rotateSpeed = 90f;

    [Tooltip("Bob amplitude for the floating effect.")]
    public float bobAmplitude = 0.1f;
    public float bobFrequency = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Rotate
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);

        // Bob up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Notify the manager and deactivate
        CollectibleManager.Instance?.Collect(collectibleType);
        gameObject.SetActive(false); // Will be replaced with object pooling in Week 4
    }
}
