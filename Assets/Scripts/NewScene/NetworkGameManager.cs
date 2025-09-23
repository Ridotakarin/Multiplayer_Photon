using System.Threading.Tasks;
using UnityEngine;
using Fusion;



public class NetworkGameManager : NetworkRunnerCall
{
    public static NetworkGameManager Instance { get; private set; }

    [Header("Network Prefabs (index matches SelectedCharacterIndex)")]
    public NetworkPrefabRef[] playerPrefabs;

    [Header("Spawn points for game scene")]
    public Transform[] SpawnPoints; // set 4 child spawn points trong inspector

    public NetworkRunner Runner { get; private set; }
    public bool IsHost { get; private set; }
    private bool callbacksAdded;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    

    public async Task<bool> StartHost(string sessionName, int sceneIndex)
    {
        return await StartRunner(GameMode.Host, sessionName, sceneIndex);
    }

    public async Task<bool> StartClient(string sessionName, int sceneIndex)
    {
        return await StartRunner(GameMode.Client, sessionName, sceneIndex);
    }

    private async Task<bool> StartRunner(GameMode mode, string sessionName, int sceneIndex)
    {
        if (Runner == null)
        {
            Runner = gameObject.AddComponent<NetworkRunner>();
            Runner.ProvideInput = true;
        }

        if (!callbacksAdded)
        {
            Runner.AddCallbacks(this);
            callbacksAdded = true;
        }

        var result = await Runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            SceneManager = Runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 8,
            Scene = SceneRef.FromIndex(sceneIndex)
        });

        if (!result.Ok)
        {
            Debug.LogError($"StartRunner failed: {result.ShutdownReason}");
            await Runner.Shutdown();
            Destroy(Runner);
            Runner = null;
            callbacksAdded = false;
            return false;
        }

        IsHost = (mode == GameMode.Host);
        return true;
    }

    #region Fusion Callbacks
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && !runner.TryGetPlayerObject(player, out var playerObj))
        {
            Vector3 pos = new Vector3(player.PlayerId * 2f, 0f, 0f);

            if (player == runner.LocalPlayer)
            {
                // Host spawn chính mình ngay với prefab đúng
                int idx = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);
                string name = PlayerPrefs.GetString("PlayerName", $"Player {player.PlayerId}");

                playerObj = runner.Spawn(playerPrefabs[idx], pos, Quaternion.identity, player);
                runner.SetPlayerObject(player, playerObj);

                var ctrl = playerObj.GetComponent<PlayerNetworkController>();
                if (ctrl != null)
                {
                    ctrl.CharacterIndex = idx;
                    ctrl.PlayerName = name;
                }

                Debug.Log($"[NetworkGameManager] Host spawn chính mình Player {player.PlayerId}, CharIndex={idx}, Name={name}");
                Debug.Log($"Player {player.PlayerId} joined");
                


            }
            else
            {
                // Client spawn tạm prefab mặc định
                playerObj = runner.Spawn(playerPrefabs[0], pos, Quaternion.identity, player);
                runner.SetPlayerObject(player, playerObj);

                Debug.Log($"[NetworkGameManager] Spawn tạm cho Client {player.PlayerId}, chờ RPC config");
                Debug.Log($"Player {player.PlayerId} joined");

                
            }
        }
    }
    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} left");

        // tìm tất cả PlayerNetworkController thuộc playerRef đó
        foreach (var p in FindObjectsOfType<PlayerNetworkController>())
        {
            if (p.Object.InputAuthority == player)
            {
                runner.Despawn(p.Object);
            }
        }

        
    }

    public override void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[NetworkGameManager] Scene loaded, player objects migrated.");
    }
    #endregion
}
