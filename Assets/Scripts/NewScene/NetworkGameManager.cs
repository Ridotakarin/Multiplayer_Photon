using System.Threading.Tasks;
using UnityEngine;
using Fusion;

public class NetworkGameManager : NetworkRunnerCall
{
    public static NetworkGameManager Instance { get; private set; }

    [Header("Network Prefabs (index matches SelectedCharacterIndex)")]
    public NetworkPrefabRef[] playerPrefabs;

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
            int charIdx;
            string playerName;

            if (player == runner.LocalPlayer) // Đây là host
            {
                charIdx = PlayerPrefs.GetInt("Host_CharacterIndex", 0);
                playerName = PlayerPrefs.GetString("Host_PlayerName", $"Host_{player.PlayerId}");
            }
            else // Đây là client join vào
            {
                charIdx = PlayerPrefs.GetInt("Client_CharacterIndex", 0);
                playerName = PlayerPrefs.GetString("Client_PlayerName", $"Client_{player.PlayerId}");
            }

            Vector3 pos = new Vector3(player.PlayerId * 2f, 0f, 0f);
            playerObj = runner.Spawn(playerPrefabs[charIdx], pos, Quaternion.identity, player);

            var ctrl = playerObj.GetComponent<PlayerNetworkController>();
            if (ctrl != null)
            {
                ctrl.CharacterIndex = charIdx;
                ctrl.PlayerName = playerName;
            }

            Debug.Log($"[NetworkGameManager] Spawn Player {player.PlayerId}, CharIdx={charIdx}, Name={playerName}");
        }
    }

    public override void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[NetworkGameManager] Scene loaded, player objects migrated.");
    }
    #endregion
}
