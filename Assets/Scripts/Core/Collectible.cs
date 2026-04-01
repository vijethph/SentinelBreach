using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectibleType { DataShard, ShieldCell, SurgeToken, GhostChip }

public class Collectible : MonoBehaviour
{
    public CollectibleType collectibleType;
    public int             value       = 10;
    public float           rotateSpeed = 90f;
    public float           bobAmp      = 0.1f;
    public float           bobFreq     = 2f;

    private Vector3 startPos;

    void Start() { startPos = transform.position; }

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
        float y = startPos.y + Mathf.Sin(Time.time * bobFreq) * bobAmp;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        CollectibleManager mgr = CollectibleManager.Instance;
        mgr?.Collect(collectibleType, other.gameObject);
        gameObject.SetActive(false);
    }
}