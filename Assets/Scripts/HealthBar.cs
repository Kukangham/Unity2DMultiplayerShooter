using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class HealthBar : NetworkBehaviour
{
    public const string BAR = "Bar";

    [SerializeField] private TextMeshProUGUI coinText;

    private NetworkVariable<int> health = new NetworkVariable<int>();
    private NetworkVariable<int> healthMax = new NetworkVariable<int>(2);
    private NetworkVariable<int> coinAmount = new NetworkVariable<int>();
    private NetworkVariable<FixedString64Bytes> coinTextValue = new NetworkVariable<FixedString64Bytes>();

    private void Start()
    {
        health.Value = healthMax.Value;
        health.OnValueChanged += (oldValue, newValue) => OnTakeDamage();
        coinTextValue.OnValueChanged += (oldValue, newValue) => coinText.text = newValue.ToString();

        transform.position = transform.position + new Vector3(0, -0.5f);
        coinText.transform.position = transform.position + new Vector3(0, -0.5f);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            health.Value = healthMax.Value;
        }
    }

    public void OnTakeDamage()
    {
        float currentHealth = (float)health.Value / healthMax.Value;
        Transform barTransform = transform.Find(BAR);
        if (barTransform != null)
        {
            barTransform.localScale = new Vector3(currentHealth, 1);
        }
        else
        {
            Debug.Log("Bar game object not found");
        }
    }

    [ClientRpc]
    public void DamageClientRpc(int damageAmount)
    {
        if (IsServer)
        {
            health.Value -= damageAmount;
        }
    }

    public int GetHealth()
    {
        return health.Value;
    }

    public void OnCollectCoin()
    {
        coinAmount.Value++;
        coinTextValue.Value = coinAmount.Value.ToString();
        if (coinText != null)
        {
            coinText.text = coinTextValue.Value.ToString();
        }
        else
        {
            Debug.Log("coinText game object not found");
        }
    }
}
