using System.Collections;
using System.Collections.Generic;
using UnityEngine;





[RequireComponent(typeof(LineRenderer), typeof(BoxCollider))]
public class LaserGridController : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 25;

    [Header("Timing")]
    public float onDuration  = 1.5f;
    public float offDuration = 1.0f;
    public float startOffset = 0f;   

    private LineRenderer lr;
    private BoxCollider  bc;
    private bool isOn = true;
	private bool coroutineRunning = false;
	
	void Start()
	{
		ApplyDifficultyHeight();
        if (!coroutineRunning)
            StartCoroutine(ToggleRoutine());
	}
	
	void ApplyDifficultyHeight()
    {
        if (DifficultyManager.Instance == null) return;

        float rawHeight = DifficultyManager.Instance.LaserHeight;
        float safeHeight = Mathf.Clamp(rawHeight, 0.2f, 1.8f);

        
        Vector3 worldPos = transform.position;
        worldPos.y = safeHeight;
        transform.position = worldPos;

        onDuration = DifficultyManager.Instance.LaserOnDuration;
    }

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        bc = GetComponent<BoxCollider>();

        
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
        
		
        isOn = false;
        if (lr != null) lr.enabled = false;
        if (bc != null) bc.enabled = false;
        coroutineRunning = false;
        StopAllCoroutines();
    }

    IEnumerator ToggleRoutine()
    {
		coroutineRunning = true;
		
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

    
    public void ForceOff(float duration) => StartCoroutine(ForceOffCoroutine(duration));

    IEnumerator ForceOffCoroutine(float dur)
    {
        StopCoroutine(nameof(ToggleRoutine));
        SetBeam(false);
        yield return new WaitForSeconds(dur);
        StartCoroutine(ToggleRoutine());
    }
	
	
	public void GhostDisable(float duration)
	{
		StopAllCoroutines();
		coroutineRunning = false;
		SetBeam(false);
		StartCoroutine(ReEnableAfter(duration));
	}

	IEnumerator ReEnableAfter(float duration)
	{
		yield return new WaitForSeconds(duration);
		StartCoroutine(ToggleRoutine());
	}
	
	
	public void ApplyStealthTiming()
	{
		offDuration = Mathf.Min(offDuration + 0.8f, 3.5f);
		onDuration  = Mathf.Max(onDuration  - 0.3f, 0.5f);
		Debug.Log("[LaserGrid] Stealth timing applied.");
	}
}