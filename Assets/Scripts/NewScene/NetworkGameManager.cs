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

    [Header("Runner")]
    [SerializeField] private GameObject runnerPrefab; // prefab chứa NetworkRunner
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

    #region Start Runner
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
        // Nếu đã có runner cũ, shutdown trước
        if (Runner != null)
        {
            await Runner.Shutdown();
            Destroy(Runner.gameObject);
            Runner = null;
            callbacksAdded = false;
            IsHost = false;
        }

        // Instantiate runner prefab mới
        GameObject runnerObj = Instantiate(runnerPrefab);
        Runner = runnerObj.GetComponent<NetworkRunner>();
        Runner.ProvideInput = true;

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

            if (Runner != null)
            {
                await Runner.Shutdown();
                Destroy(Runner.gameObject);
            }

            Runner = null;
            callbacksAdded = false;
            IsHost = false;

            //// Trở lại menu
            //UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
            return false;
        }

        IsHost = (mode == GameMode.Host);
        return true;
    }
    #endregion

    #region Fusion Callbacks
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && !runner.TryGetPlayerObject(player, out var playerObj))
        {
            Vector3 pos = new Vector3(player.PlayerId * 2f, 0f, 0f);

            if (player == runner.LocalPlayer)
            {
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
            }
            else
            {
                playerObj = runner.Spawn(playerPrefabs[0], pos, Quaternion.identity, player);
                runner.SetPlayerObject(player, playerObj);

                Debug.Log($"[NetworkGameManager] Spawn tạm cho Client {player.PlayerId}, chờ RPC config");
            }

            Debug.Log($"Player {player.PlayerId} joined");

            // Gọi sang GameLogic (nếu có trong scene)
            var gameLogic = FindObjectOfType<GameLogic>();
            Debug.Log($"GameLogic: get {gameLogic}");
            if (gameLogic != null && playerObj != null)
            {
                var ctrl = playerObj.GetComponent<PlayerNetworkController>();
                if (ctrl != null)
                {
                    gameLogic.OnPlayerJoined(ctrl);
                }
            }
        }
    }


    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player.PlayerId} left");

        foreach (var p in FindObjectsOfType<PlayerNetworkController>())
        {
            if (p.Object.InputAuthority == player)
            {
                runner.Despawn(p.Object);
            }
        }

        // Nếu host rời => shutdown room
        if (runner.IsServer && player == runner.LocalPlayer)
        {
            Debug.Log("Host left -> shutting down room...");
            runner.Shutdown();
        }
    }

    public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"Runner shutdown: {shutdownReason}");
        Runner = null;
        IsHost = false;
        Instance = null;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Đưa tất cả về menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }

    public override void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[NetworkGameManager] Scene loaded, player objects migrated.");
    }
    #endregion

    #region Quit Application
    private async void OnApplicationQuit()
    {
        if (Runner != null)
        {
            Debug.Log("[NetworkGameManager] Application quitting → shutting down Runner...");
            await Runner.Shutdown();
            Runner = null;
            IsHost = false;
            Instance = null;
        }
    }
    #endregion
}
