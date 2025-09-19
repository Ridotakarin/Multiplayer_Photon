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

    [Networked] public string PlayerName { get; set; }
    [Networked] public int CharacterIndex { get; set; }

    public override void Spawned()
    {
        _controller = GetComponent<NetworkCharacterController>();

        Debug.Log($"[Spawned] Player {Object.InputAuthority} - Name={PlayerName}, CharIndex={CharacterIndex}");

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

    // === RPCs để sync dữ liệu từ client lên StateAuthority ===
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SetCharacterIndex(int idx)
    {
        CharacterIndex = idx;
        Debug.Log($"[Server] Player {Object.InputAuthority.PlayerId} set CharIndex={idx}");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_SetPlayerName(string name)
    {
        PlayerName = name;
        Debug.Log($"[Server] Player {Object.InputAuthority.PlayerId} set Name={name}");
    }

    // === Spawn & Respawn ===
    public void SetInitialSpawnPoint(Vector3 position, Quaternion rotation)
    {
        if (Object.HasStateAuthority)
        {
            InitialSpawnPosition = position;
            InitialSpawnRotation = rotation;
            transform.SetPositionAndRotation(InitialSpawnPosition, InitialSpawnRotation);
            Debug.Log($"[Server] Set spawn point cho {Object.InputAuthority} tại {position}");
        }
    }

    public void Respawn()
    {
        if (Object.HasStateAuthority)
        {
            transform.SetPositionAndRotation(InitialSpawnPosition, InitialSpawnRotation);
            Debug.Log($"[Server] Respawn player {Object.InputAuthority} tại {InitialSpawnPosition}");
        }
    }
}
