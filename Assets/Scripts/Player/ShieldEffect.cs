using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class ShieldEffect : MonoBehaviour
{
    public static ShieldEffect Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("References")]
    [Tooltip("The ShieldSphere child GameObject on CIPHER.")]
    public GameObject shieldSphere;

    [Tooltip("Duration of the shield in seconds.")]
    public float shieldDuration = 5f;

    private PlayerHealth playerHealth;
    private Coroutine activeShield;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        if (shieldSphere != null) shieldSphere.SetActive(false);
    }

    
    
    
    public void ActivateShield()
    {
        if (activeShield != null) StopCoroutine(activeShield);
        activeShield = StartCoroutine(ShieldCoroutine());
    }

    IEnumerator ShieldCoroutine()
    {
        Debug.Log("[ShieldEffect] Shield activated.");

        
        if (shieldSphere != null) shieldSphere.SetActive(true);
        playerHealth?.SetInvincible(true, shieldDuration);

        
        float elapsed = 0f;
        Vector3 baseScale = shieldSphere != null ? shieldSphere.transform.localScale : Vector3.one;

        while (elapsed < shieldDuration)
        {
            elapsed += Time.deltaTime;

            if (shieldSphere != null)
            {
                float pulse = 1f + 0.04f * Mathf.Sin(elapsed * 6f);
                shieldSphere.transform.localScale = baseScale * pulse;

                
                if (elapsed > shieldDuration - 1f)
                {
                    Renderer r = shieldSphere.GetComponent<Renderer>();
                    if (r != null)
                    {
                        Color c = r.material.color;
                        c.a = Mathf.Lerp(0.24f, 0f, (elapsed - (shieldDuration - 1f)));
                        r.material.color = c;
                    }
                }
            }
            yield return null;
        }

        
        if (shieldSphere != null)
        {
            
            Renderer r = shieldSphere.GetComponent<Renderer>();
            if (r != null)
            {
                Color c = r.material.color;
                c.a = 0.24f;
                r.material.color = c;
            }
            shieldSphere.SetActive(false);
        }

        Debug.Log("[ShieldEffect] Shield expired.");
        activeShield = null;
    }
}
