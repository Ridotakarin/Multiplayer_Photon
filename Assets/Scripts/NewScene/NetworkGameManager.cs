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
            int idx = (player == runner.LocalPlayer) ? MenuManager.SelectedCharacterIndex : 0;
            if (idx < 0 || idx >= playerPrefabs.Length) idx = 0;

            Vector3 pos = new Vector3(player.PlayerId * 2f, 0f, 0f);
            playerObj = runner.Spawn(playerPrefabs[idx], pos, Quaternion.identity, player);

            var ctrl = playerObj.GetComponent<PlayerNetworkController>();
            if (ctrl != null)
            {
                ctrl.CharacterIndex = idx;
                ctrl.PlayerName = (player == runner.LocalPlayer) ? MenuManager.PlayerName : $"Player {player.PlayerId}";
            }

            Debug.Log($"[NetworkGameManager] Spawn Player {player.PlayerId}, Name={ctrl.PlayerName}, CharIndex={ctrl.CharacterIndex}");
        }
    }

    public override void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[NetworkGameManager] Scene loaded, player objects migrated.");
    }
    #endregion
}
