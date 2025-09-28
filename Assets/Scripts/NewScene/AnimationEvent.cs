using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    private PlayerNetworkController controller;

    private void Awake()
    {
        controller = GetComponentInParent<PlayerNetworkController>();
    }

    // Gọi từ animation event
    public void OnAttackAnimationHit()
    {
        if (controller != null && controller.Object.HasInputAuthority)
        {
            controller.RequestProjectile = true; // set flag
        }
    }
}
