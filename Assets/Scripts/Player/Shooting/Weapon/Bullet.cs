using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float timeToDestroy;
    [HideInInspector] public WeaponManager weapon;
    [HideInInspector] public Vector3 dir;
    
    private Rigidbody rb;
    private bool isPaused;
    private Vector3 savedVelocity;
    private Vector3 savedAngularVelocity;
    private float remainingTimeToDestroy;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        remainingTimeToDestroy = timeToDestroy;
        
        EventManager.Instance.OnDroneCamActivated += OnDroneCamActivated;
        EventManager.Instance.OnDroneCamDeactivated += OnDroneCamDeactivated;
        EventManager.Instance.OnGamePaused += OnGamePaused;
        EventManager.Instance.OnGameContinued += OnGameContinued;
        
        StartCoroutine(DestroyAfterDelay());
    }

    private void OnDestroy()
    {
        EventManager.Instance.OnDroneCamActivated -= OnDroneCamActivated;
        EventManager.Instance.OnDroneCamDeactivated -= OnDroneCamDeactivated;
        EventManager.Instance.OnGamePaused -= OnGamePaused;
        EventManager.Instance.OnGameContinued -= OnGameContinued;
    }


    private IEnumerator DestroyAfterDelay()
    {
        while (remainingTimeToDestroy > 0)
        {
            // Pause handling
            while (isPaused)
            {
                yield return null; // Wait while paused
            }

            remainingTimeToDestroy -= Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
    
    private void OnCollisionEnter(Collision other)
    {
        var enemyHealth = other.gameObject.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            Vector3 hitPoint = other.contacts[0].point; // Get the exact point of collision
            Vector3 burstDirection = -transform.forward;

            if (enemyHealth.bloodSplatterPrefab != null)
            {
                ParticleSystem bloodSplatter = Instantiate(enemyHealth.bloodSplatterPrefab, hitPoint, Quaternion.identity);
                bloodSplatter.transform.rotation = Quaternion.LookRotation(burstDirection);
                bloodSplatter.Play();
                Destroy(bloodSplatter.gameObject, bloodSplatter.main.duration);
            }
            
            if (!enemyHealth.isDead)
            {
                enemyHealth.TakeDamage(weapon.damage);
                
                if (enemyHealth.Health <= 0.0f)
                {
                    var enemyController = other.gameObject.GetComponentInParent<EnemyController>();

                    if (enemyController != null && (enemyController.CurrentState != enemyController.Run && enemyController.CurrentState != enemyController.Scream))
                    {
                        Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
                        rb.AddForce(dir * weapon.enemyKickbackForce, ForceMode.Impulse);
                        enemyHealth.isDead = true;
                    }
                }
            }
            else // might not be of use, but still reassurance
            {
                // The enemy is already dead: stop the bullet's velocity to avoid affecting the ragdoll
                Rigidbody bulletRb = GetComponent<Rigidbody>();
                bulletRb.velocity = Vector3.zero;
                bulletRb.isKinematic = true;
            }
            
            Destroy(gameObject);
        }
    }
    
    private void PauseBullet()
    {
        if (rb != null)
        {
            savedVelocity = rb.velocity;
            savedAngularVelocity = rb.angularVelocity;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true; // Disable physics simulation
        }

        isPaused = true;
    }

    private void ResumeBullet()
    {
        if (rb != null)
        {
            rb.isKinematic = false; // Re-enable physics simulation
            rb.velocity = savedVelocity;
            rb.angularVelocity = savedAngularVelocity;
        }

        isPaused = false;
    }
    
    private void OnDroneCamActivated()
    {
        isPaused = true;
        PauseBullet();
    }

    private void OnDroneCamDeactivated()
    {
        isPaused = false;
        ResumeBullet();
    }

    private void OnGamePaused()
    {
        isPaused = true;
        PauseBullet();
    }

    private void OnGameContinued()
    {
        isPaused = false;
        ResumeBullet();
    }


}
