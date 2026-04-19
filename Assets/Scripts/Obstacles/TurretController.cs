using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class TurretController : MonoBehaviour
{
    [Header("References")]
    public Transform turretHead;
    public Transform muzzle;

    [Header("Settings")]
    public float detectionRange  = 14f;   
    public float fireInterval    = 1.2f;
    public float rotationSpeed   = 12f;   
    public int   damage          = 20;

    [Header("Laser Flash VFX")]
    [Tooltip("Duration in seconds the laser line is visible after each shot.")]
    public float laserFlashDuration = 0.12f;

    
    private Transform player;
    private float     fireTimer;
    private bool      isDisabled;
    private LineRenderer laserLine;

    

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogWarning("[TurretController] No Player tag found.");

        fireTimer = Random.Range(0f, fireInterval);
		
		
		if (DifficultyManager.Instance != null)
		{
			rotationSpeed = DifficultyManager.Instance.TurretRotationSpeed;
			fireInterval  = DifficultyManager.Instance.TurretFireInterval;

			
			float mountY = DifficultyManager.Instance.TurretMountHeight;
			transform.localPosition = new Vector3(
				transform.localPosition.x,
				mountY,
				transform.localPosition.z
			);
		}

        
        laserLine = gameObject.AddComponent<LineRenderer>();
        laserLine.positionCount    = 2;
        laserLine.startWidth       = 0.03f;
        laserLine.endWidth         = 0.03f;
        laserLine.useWorldSpace    = true;
        laserLine.enabled          = false;
        laserLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        
        Material laserMat = Resources.Load<Material>("Mat_LaserBeam");
        if (laserMat == null)
        {
            
            laserMat = new Material(Shader.Find("Standard"));
            laserMat.color = new Color(1f, 0.2f, 0f);
            laserMat.EnableKeyword("_EMISSION");
            laserMat.SetColor("_EmissionColor", new Color(1f, 0.2f, 0f) * 3f);
        }
        laserLine.material = laserMat;
    }

    void Update()
    {
        if (isDisabled || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return;

        TrackPlayer();

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            Fire();
            fireTimer = fireInterval;
        }
    }

    void TrackPlayer()
    {
        if (turretHead == null) return;
        Vector3 dir = (player.position - turretHead.position).normalized;
        Quaternion target = Quaternion.LookRotation(dir);
        turretHead.rotation = Quaternion.Slerp(
            turretHead.rotation, target,
            rotationSpeed * Time.deltaTime
        );
    }

    void Fire()
    {
        if (muzzle == null) return;

        Vector3 fireDir = (player.position - muzzle.position).normalized;
        Ray ray = new Ray(muzzle.position, fireDir);

        
        StartCoroutine(ShowLaserFlash(muzzle.position,
            muzzle.position + fireDir * detectionRange));

        
		if (Physics.Raycast(muzzle.position, fireDir, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<PlayerHealth>()
                    ?.TakeDamage(damage, muzzle.position);
            }
        }
    }

    IEnumerator ShowLaserFlash(Vector3 start, Vector3 end)
    {
        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);
        laserLine.enabled = true;
        yield return new WaitForSeconds(laserFlashDuration);
        laserLine.enabled = false;
    }

    public void Disable(float duration) => StartCoroutine(DisableCoroutine(duration));

    IEnumerator DisableCoroutine(float duration)
    {
        isDisabled = true;
        if (turretHead != null) turretHead.gameObject.SetActive(false);
        yield return new WaitForSeconds(duration);
        isDisabled = false;
        if (turretHead != null) turretHead.gameObject.SetActive(true);
    }
	
	
    public void ApplyStealthModifier()
    {
        rotationSpeed = Mathf.Max(rotationSpeed - 4f, 2f);
    }
}