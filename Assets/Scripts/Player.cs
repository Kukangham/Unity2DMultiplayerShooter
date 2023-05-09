using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public static Player LocalInstance { get; private set; }

    private Vector2 smoothedMoveInput;
    private Vector2 moveInputSmoothVelocity;
    private float moveSpeed = 7f;
    private float rotationSpeed = 600f;

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        HandleMovement();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;
        }

        Vector3 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 0));
        spawnPosition.z = 0;
        transform.position = spawnPosition;
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, inputVector.y, 0f);

        float smoothTime = .1f;
        smoothedMoveInput = Vector2.SmoothDamp(smoothedMoveInput, moveDir, ref moveInputSmoothVelocity, smoothTime);

        Vector3 smoothedMoveInput3D = new Vector3(smoothedMoveInput.x, smoothedMoveInput.y, 0f);
        transform.position += smoothedMoveInput3D * moveSpeed * Time.deltaTime;

        RotateDirection(inputVector, smoothedMoveInput3D);

        KeepPlayerWithinScreenBounds();
    }

    private void KeepPlayerWithinScreenBounds()
    {
        Vector3 position = transform.position;
        Vector3 viewportPosition = Camera.main.WorldToViewportPoint(position);

        CircleCollider2D collider = GetComponent<CircleCollider2D>();

        // Calculate the size of the collider in viewport coordinates
        Vector3 colliderSize = Camera.main.WorldToViewportPoint(new Vector3(collider.radius * 2, collider.radius * 2, 0)) - Camera.main.WorldToViewportPoint(Vector3.zero);

        // Calculate the min and max values for the viewport position
        float min = colliderSize.x / 2;
        float maxX = 1 - min;
        float maxY = 1 - min;

        // Clamp the viewport position
        viewportPosition.x = Mathf.Clamp(viewportPosition.x, min, maxX);
        viewportPosition.y = Mathf.Clamp(viewportPosition.y, min, maxY);

        transform.position = Camera.main.ViewportToWorldPoint(viewportPosition);
    }

    private void RotateDirection(Vector2 inputVector, Vector3 smoothedMoveInput3D)
    {
        if (inputVector != Vector2.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.forward, smoothedMoveInput);
            Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            transform.rotation = rotation;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Coin"))
        {
            CollectCoinClientRpc();

            if (IsServer)
            {
                Destroy(collision.gameObject);
                CoinSpawner.Instance.SpawnCoin();
            }
        }
    }

    [ClientRpc]
    public void CollectCoinClientRpc()
    {
        if (IsServer)
        {
            HealthBar healthBar = gameObject.GetComponentInChildren<HealthBar>();
            healthBar.OnCollectCoin();
        }
    }
}
