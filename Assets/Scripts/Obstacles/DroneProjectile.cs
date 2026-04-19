using System.Collections;
using System.Collections.Generic;
using UnityEngine;










[RequireComponent(typeof(SphereCollider))]
public class DroneProjectile : MonoBehaviour
{
    [Header("Physics Constants (student-written)")]
    [Tooltip("Horizontal launch speed in m/s.")]
    public float launchSpeed = 10f;

    [Tooltip("Gravity applied to the projectile (m/s²). Negative = downward.")]
    public float gravity = -18f;

    [Tooltip("Projectile lifetime before auto-destroy.")]
    public float lifetime = 2.5f;

    [Tooltip("Damage dealt to CIPHER on hit.")]
    public int damage = 15;

    
    private Vector3 velocity;       
    private float   verticalVel;    
    private float   age = 0f;
    private bool    hasHit = false;

    

    
    
    
    public void Launch(Vector3 direction, float upAngle = 5f)
    {
        
        direction.y = Mathf.Tan(upAngle * Mathf.Deg2Rad);
        direction    = direction.normalized;

        velocity    = direction * launchSpeed;
        verticalVel = velocity.y;
        velocity.y  = 0f;   
    }

    void Update()
    {
        if (hasHit) return;

        age += Time.deltaTime;
        if (age >= lifetime) { Destroy(gameObject); return; }

        
        
        
        Vector3 horizontalMove = velocity * Time.deltaTime;

        
        
        
        verticalVel += gravity * Time.deltaTime;
        float verticalMove = verticalVel * Time.deltaTime;

        
        transform.position += horizontalMove + new Vector3(0f, verticalMove, 0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            hasHit = true;
            other.GetComponent<PlayerHealth>()
                 ?.TakeDamage(damage, transform.position);
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy") && !other.CompareTag("Projectile"))
        {
            
            hasHit = true;
            Destroy(gameObject);
        }
    }
}