using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Networked] private TickTimer LifeTimer { get; set; }
    [Networked] private float Speed { get; set; }
    [Networked] private Vector3 Direction { get; set; }

    // Host gọi khi spawn projectile
    public void Init(Vector3 dir, float spd, float lifeTime = 5f)
    {
        if (Object.HasStateAuthority)
        {
            Direction = dir.normalized;
            Speed = spd;
            LifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            transform.position += Direction * Speed * Runner.DeltaTime;

            if (LifeTimer.Expired(Runner))
                Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Object.HasStateAuthority) return;

        var player = other.GetComponentInParent<PlayerNetworkController>();
        if (player != null)
        {
            player.Respawn();
            Runner.Despawn(Object);
        }
    }
}
