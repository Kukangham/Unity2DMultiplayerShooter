using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnFire;
    public event EventHandler OnFireCanceled;

    private PlayerInputActions playerInputActions;
    public bool isSingleFireMode;
    private bool isFiring;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Fire.performed += Fire_performed;
        playerInputActions.Player.Fire.canceled += Fire_canceled;
    }

    private void Update()
    {
        if (isFiring)
        {
            OnFire?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Fire_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isFiring = false;
        OnFireCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Fire_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        isFiring = true;
        isSingleFireMode = true;
        StartCoroutine(SingleFireModeCoroutine());
    }

    private IEnumerator SingleFireModeCoroutine()
    {
        yield return new WaitForSeconds(0.001f);
        isSingleFireMode = false;
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }
}
