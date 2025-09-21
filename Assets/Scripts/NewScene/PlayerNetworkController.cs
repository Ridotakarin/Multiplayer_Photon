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

        if (Object.HasInputAuthority)
        {
           

            string nameKey = Runner.IsServer ? "Host_PlayerName" : "Client_PlayerName";
            string idxKey = Runner.IsServer ? "Host_CharacterIndex" : "Client_CharacterIndex";

            PlayerName = PlayerPrefs.GetString(nameKey, $"Player {Object.InputAuthority.PlayerId}");
            CharacterIndex = PlayerPrefs.GetInt(idxKey, 0);

            Rpc_PlayerConfig(CharacterIndex, PlayerName);
        }

        Debug.Log($"[Spawned] Player {Object.InputAuthority} - Name={PlayerName}, CharIndex={CharacterIndex}");

        // Màu sắc test trực quan
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

    // === RPCs từ client báo về host ===
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_PlayerConfig(int idx, string name)
    {
        CharacterIndex = idx;
        PlayerName = name;
        Debug.Log($"[Server] Player {Object.InputAuthority.PlayerId} set CharIndex={idx}  Name={name}");
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
