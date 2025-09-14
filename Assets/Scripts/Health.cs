using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class Health : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(HealthChanged))]
    public float NetworkHealth { get; set; } = 100;

    [SerializeField]
    private Slider _healthSlider;

    public void HealthChanged()
    {
        print($"Health changed to {NetworkHealth}");
        _healthSlider.value = NetworkHealth / 100f;
    }

    [ContextMenu("Deal 10 damage")]
    private void Deal1Damage() => DealDamageRpc(10);

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void DealDamageRpc(float damage)
    {
        // The code inside here will run on the client which owns this object (has state and input authority).
        Debug.Log("Received DealDamageRpc on StateAuthority, modifying Networked variable");
        NetworkHealth -= damage;
        print("New health: " + NetworkHealth);
    }
}
