using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerShoot : NetworkBehaviour
{
    private const string BULLET_POSITION = "BulletPosition";

    [SerializeField] private GameObject bulletPrefab;

    private Transform bulletPositionTransform;
    private float bulletSpeed = 8f;
    private float timeBetweenShots = .25f;
    private float lastFireTime;
    private bool hasFiredInSingleFireMode;

    private void Awake()
    {
        bulletPositionTransform = transform.Find(BULLET_POSITION);
    }

    private void Start()
    {
        GameInput.Instance.OnFire += PlayerShoot_OnFireAction;
        GameInput.Instance.OnFireCanceled += PlayerShoot_OnFireCanceled;
    }

    private void PlayerShoot_OnFireCanceled(object sender, EventArgs e)
    {
        hasFiredInSingleFireMode = false;
    }

    private void PlayerShoot_OnFireAction(object sender, EventArgs e)
    {
        if (!IsOwner) return;

        PlayerShoot_OnFireActionServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerShoot_OnFireActionServerRpc()
    {
        float timeSinceLastFire = Time.time - lastFireTime;
        if (timeSinceLastFire >= timeBetweenShots && (!GameInput.Instance.isSingleFireMode || !hasFiredInSingleFireMode))
        {
            if (bulletPositionTransform != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletPositionTransform.position, transform.rotation);
                bullet.GetComponent<NetworkObject>().Spawn(true);

                NetworkRigidbody2D rigidbody = bullet.GetComponent<NetworkRigidbody2D>();
                rigidbody.GetComponent<Rigidbody2D>().velocity = bulletSpeed * transform.up;

                lastFireTime = Time.time;
                hasFiredInSingleFireMode = true;
            }
        }
    }
}
