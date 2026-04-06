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

	void Start()
	{
		startPos = transform.position;
		// Switch to continuous detection so fast-moving CIPHER
		// doesn't clip through the trigger without registering
		Rigidbody rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
			rb.isKinematic = true;   // collectible doesn't move physically
			rb.useGravity  = false;
			rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
		}
	}

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