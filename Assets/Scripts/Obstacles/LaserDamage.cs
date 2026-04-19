using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class LaserDamage : MonoBehaviour
{
    [Tooltip("HP damage dealt to the player per hit.")]
    public int damageAmount = 25;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
			{
               ph.TakeDamage(damageAmount, transform.position);
			   AudioManager.Instance?.PlayLaser();
			}
        }
    }
}