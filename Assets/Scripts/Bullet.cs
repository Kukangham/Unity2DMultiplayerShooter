using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    private int damageAmount = 1;
    public PlayerShoot bulletParent;

    private void Update()
    {
        CapsuleCollider2D collider = GetComponent<CapsuleCollider2D>();

        // Calculate the size of the collider in viewport coordinates
        Vector3 colliderSize = Camera.main.WorldToViewportPoint(collider.size) - Camera.main.WorldToViewportPoint(Vector3.zero);

        // Calculate the min and max values for the viewport position
        float minX = -colliderSize.x / 2;
        float maxX = 1 - minX;
        float minY = -colliderSize.y / 2;
        float maxY = 1 - minY;

        // Check if the bullet is off-screen
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPosition.x < minX || viewportPosition.x > maxX || viewportPosition.y < minY || viewportPosition.y > maxY)
        {
            DestroyBulletServerRpc();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<Player>())
        {
            HealthBar healthBar = collision.gameObject.GetComponentInChildren<HealthBar>();
            healthBar.DamageClientRpc(damageAmount);

            if (healthBar.GetHealth() <= 0)
            {
                Destroy(collision.gameObject);
                Destroy(healthBar);
            }

            DestroyBulletServerRpc();

            healthBar.OnTakeDamage();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyBulletServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
