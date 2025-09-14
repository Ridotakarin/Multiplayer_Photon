using UnityEngine;
using Fusion;

public class RaycastAttack : NetworkBehaviour
{
    [SerializeField] private float _damage;

    private Camera _aimingCamera;

    private void Start() => _aimingCamera = Camera.main;

    private void Update()
    {
        if (!HasStateAuthority) return;

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            PerformRaycast();
        }
    }

    private void PerformRaycast()
    {
        Ray ray = _aimingCamera.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red, 1);
        if (Physics.Raycast(ray, out var hit)
            && hit.collider.gameObject != gameObject
            && hit.collider.TryGetComponent<Health>(out var health))
        {
            health.DealDamageRpc(_damage);
            health.HealthChanged();
        }
    }
}
