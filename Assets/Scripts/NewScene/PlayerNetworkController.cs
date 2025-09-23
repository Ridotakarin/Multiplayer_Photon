using Fusion;
using System.Collections;
using TMPro;
using Unity.AppUI.Core;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkCharacterController))]
public class PlayerNetworkController : NetworkBehaviour
{
    private NetworkCharacterController _controller;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameObject canvas;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private CinemachineCamera vCam;


    [Networked] public Vector3 InitialSpawnPosition { get; set; }
    [Networked] public Quaternion InitialSpawnRotation { get; set; }

    [Networked] public string PlayerName { get; set; }
    [Networked] public int CharacterIndex { get; set; }
    [Networked] private NetworkBool IsConfigured { get; set; }

    [SerializeField] private GameObject[] characterModels;

    public bool isReady;

    public override void Spawned()
    {
        _controller = GetComponent<NetworkCharacterController>();
        canvas.SetActive(true);
        if (Object.HasInputAuthority && !Object.HasStateAuthority)
        {
            // chỉ client mới cần gửi RPC lên host
            string defaultName = $"Player {Object.InputAuthority.PlayerId}";
            string name = PlayerPrefs.GetString("PlayerName", defaultName);
            int idx = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
            Rpc_RequestRespawn(idx, name);
            Runner.GetComponent<InputProvider>().LocalPlayer = this;


        }

        Debug.Log($"[Spawned] Player {Object.InputAuthority} - Name={PlayerName}, CharIndex={CharacterIndex}");

        // Test màu để phân biệt local vs remote
        if (Object.HasInputAuthority)
        {
            GetComponentInChildren<Renderer>().material.color = Color.blue;
            vCam.enabled = true;
        }
        else
        {
            GetComponentInChildren<Renderer>().material.color = Color.red;
            vCam.enabled = false;
        }
    }

    public override void Render()
    {
        if (playerNameText != null)
            playerNameText.text = PlayerName;
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
            if (input.ready && !isReady)
            {
                RPC_Ready();
            }
        }
    }

    // === RPC client -> host ===
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_RequestRespawn(int idx, string name)
    {

        if (!Runner.IsServer) return;


        var oldObj = Object;
        var pos = oldObj.transform.position;
        var rot = oldObj.transform.rotation;

        // Chỉ respawn nếu đây là placeholder (vd CharacterIndex = 0 mặc định)
        if (CharacterIndex != idx || string.IsNullOrEmpty(PlayerName))
        {
            var newObj = Runner.Spawn(
                NetworkGameManager.Instance.playerPrefabs[idx],
                pos, rot, Object.InputAuthority
            );

            var ctrl = newObj.GetComponent<PlayerNetworkController>();
            ctrl.CharacterIndex = idx;
            ctrl.PlayerName = name;
            ctrl.IsConfigured = true;

            Runner.SetPlayerObject(Object.InputAuthority, newObj);
            Runner.Despawn(oldObj);
        }
        else
        {
            CharacterIndex = idx;
            PlayerName = name;
            IsConfigured = true;
        }
    }
    [Rpc(RpcSources.InputAuthority,RpcTargets.InputAuthority | RpcTargets.StateAuthority)]
    public void RPC_Ready()
    {
        isReady = true;
        if(HasInputAuthority)
        {
            //UIManager.Singelton.DidSetReady();
        }
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
