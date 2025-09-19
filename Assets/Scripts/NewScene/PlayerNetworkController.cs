using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkCharacterController))]
public class PlayerNetworkController : NetworkBehaviour
{
    private NetworkCharacterController _controller;

    [SerializeField] private float moveSpeed = 5f;

    [Networked] public Vector3 InitialSpawnPosition { get; set; }
    [Networked] public Quaternion InitialSpawnRotation { get; set; }

    [Networked] public NetworkString<_16> PlayerName { get; set; }
    [Networked] public int CharacterIndex { get; set; }

    public override void Spawned()
    {
        _controller = GetComponent<NetworkCharacterController>();

        // Debug: màu khác nhau giữa local & remote
        if (Object.HasInputAuthority)
            GetComponentInChildren<Renderer>().material.color = Color.blue;
        else
            GetComponentInChildren<Renderer>().material.color = Color.red;
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            Vector3 move = new Vector3(input.horizontal, 0, input.vertical);
            _controller.Move(move.normalized * moveSpeed);

            if (move != Vector3.zero)
                transform.forward = move.normalized;

            if (input.jump)
                _controller.Jump();
        }
    }

    // Chỉ server mới được gọi
    public void SetInitialSpawnPoint(Vector3 position, Quaternion rotation)
    {
        if (Object.HasStateAuthority)
        {
            InitialSpawnPosition = position;
            InitialSpawnRotation = rotation;
            transform.SetPositionAndRotation(InitialSpawnPosition, InitialSpawnRotation);
        }
    }

    public void Respawn()
    {
        if (Object.HasStateAuthority)
        {
            transform.SetPositionAndRotation(InitialSpawnPosition, InitialSpawnRotation);
        }
    }
}
