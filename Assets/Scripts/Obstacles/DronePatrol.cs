using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class DronePatrol : MonoBehaviour
{
	[Header("Base values (overridden by DifficultyManager at runtime)")]
	public float baseAmplitude   = 1.0f;
	public float basePatrolSpeed = 1.2f;
	public float baseHeight      = 2.0f;

    [Header("Patrol")]
    public float amplitude  = 2.0f;    
    public float frequency  = 1.5f;    
	private float patrolSpeed;
	private float patrolHeight;

    [Header("Bob (up/down float)")]
    public float bobAmplitude = 0.2f;
    public float bobFrequency = 2.0f;

    [Header("Damage")]
    public int   damage         = 15;
    public float damageCooldown = 1.0f;

    private Vector3 originPosition;
    private float   damageTimer = 0f;
    private bool    isDisabled  = false;
    private float   phaseOffset;
	
	[Header("Projectile Attack (level 3+)")]
	public GameObject projectilePrefab;

	[Tooltip("Seconds between shots.")]
	public float fireInterval = 2.5f;

	[Tooltip("Minimum CIPHER level before drone fires projectiles.")]
	public int firesAtLevel = 3;

	private float fireTimer = 0f;
	private Transform player;

    void Start()
    {
		
		if (DifficultyManager.Instance != null)
		{
			amplitude    = DifficultyManager.Instance.DroneAmplitude;
			patrolSpeed  = DifficultyManager.Instance.DronePatrolSpeed;
			patrolHeight = DifficultyManager.Instance.DronePatrolHeight;
		}
		else
		{
			amplitude    = baseAmplitude;
			patrolSpeed  = basePatrolSpeed;
			patrolHeight = baseHeight;
		}
		
		    
		Vector3 pos = transform.localPosition;
		transform.localPosition = new Vector3(pos.x, patrolHeight, pos.z);
	
		player = GameObject.FindGameObjectWithTag("Player")?.transform;
		fireTimer = Random.Range(0f, fireInterval);  
		
        originPosition = transform.position;
        phaseOffset    = Random.Range(0f, Mathf.PI * 2f); 
    }

    void Update()
    {
        if (isDisabled) return;

        
        float newX = originPosition.x + Mathf.Sin(Time.time * frequency + phaseOffset) * amplitude;
        float newY = originPosition.y + Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(newX, newY, transform.position.z);
		
		
		float sineOffset = Mathf.Sin(Time.time * patrolSpeed) * amplitude;
		transform.localPosition = new Vector3(
			sineOffset,
			patrolHeight,
			transform.localPosition.z
		);

        if (damageTimer > 0f) damageTimer -= Time.deltaTime;
		
		if (projectilePrefab != null && player != null)
		{
			int level = PlayerPrefs.GetInt("CipherLevel", 1);
			if (level >= firesAtLevel)
			{
				fireTimer -= Time.deltaTime;
				if (fireTimer <= 0f)
				{
					FireAtPlayer();
					fireTimer = fireInterval;
				}
			}
		}
    }
	
	void FireAtPlayer()
	{
		if (player == null || isDisabled) return;

		Vector3 dir = (player.position - transform.position).normalized;
		GameObject proj = Instantiate(projectilePrefab,
			transform.position + dir * 0.3f, Quaternion.identity);

		proj.GetComponent<DroneProjectile>()?.Launch(dir);
	}

    void OnTriggerEnter(Collider other)
    {
        if (isDisabled || damageTimer > 0f) return;
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        ph?.TakeDamage(damage, transform.position);
        damageTimer = damageCooldown;
    }

    
    public void Disable(float duration) => StartCoroutine(DisableCoroutine(duration));

    IEnumerator DisableCoroutine(float dur)
    {
        isDisabled = true;
        yield return new WaitForSeconds(dur);
        isDisabled = false;
    }
}