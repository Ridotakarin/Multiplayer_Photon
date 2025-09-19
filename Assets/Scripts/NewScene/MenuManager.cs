using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : NetworkRunnerCall
{
    public static MenuManager Instance;

    [Header("UI Components")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    [Header("UI Control")]
    [SerializeField] private GameObject lobbyUI;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListPrefab;

    [Header("Character Selection UI")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Button previousCharacterButton;
    [SerializeField] private Button nextCharacterButton;
    [SerializeField] private GameObject[] characterDisplayObjects;
    [SerializeField] private string[] characterNames;

    [Header("Network Prefabs (index matches SelectedCharacterIndex)")]
    public NetworkPrefabRef[] playerPrefabs;

    [Header("Build index of gameplay scene (set in Build Settings)")]
    public int gameSceneBuildIndex = 1;

    // runtime
    public static string PlayerName;
    public static int SelectedCharacterIndex = 0;

    private NetworkRunner _runner;
    private bool runnerStarted = false;
    private bool callbacksAdded = false;

    private readonly Dictionary<PlayerRef, bool> readyStates = new();
    private readonly Dictionary<PlayerRef, int> playerSelections = new();

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
            return;
        }

        // UI listeners
        nameInputField?.onValueChanged.AddListener(OnNameInputChanged);
        previousCharacterButton?.onClick.AddListener(OnPreviousCharacterClicked);
        nextCharacterButton?.onClick.AddListener(OnNextCharacterClicked);

        hostButton?.onClick.AddListener(OnHostButtonClicked);
        joinButton?.onClick.AddListener(OnJoinButtonClicked);
        readyButton?.onClick.AddListener(OnReadyClicked);
        leaveButton?.onClick.AddListener(OnLeaveClicked);
        startButton?.onClick.AddListener(OnStartClicked);

        lobbyUI?.SetActive(false);
        startButton?.gameObject.SetActive(false);

        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner != null) _runner.ProvideInput = false;
    }

    private void Start()
    {
        SelectedCharacterIndex = 0;
        UpdateCharacterDisplay();
        UpdateCreateJoinInteractivity();
    }

    private void OnDestroy()
    {
        if (_runner != null && callbacksAdded)
        {
            _runner.RemoveCallbacks(this);
            callbacksAdded = false;
        }
    }

    #region Character UI
    private void OnNameInputChanged(string newName)
    {
        PlayerName = newName?.Trim() ?? string.Empty;
        UpdateCreateJoinInteractivity();
    }

    private void UpdateCreateJoinInteractivity()
    {
        bool ok = !string.IsNullOrEmpty(PlayerName);
        if (hostButton != null) hostButton.interactable = ok;
        if (joinButton != null) joinButton.interactable = ok;
    }

    private void OnPreviousCharacterClicked()
    {
        if (characterDisplayObjects == null || characterDisplayObjects.Length == 0) return;
        SelectedCharacterIndex = (SelectedCharacterIndex - 1 + characterDisplayObjects.Length) % characterDisplayObjects.Length;
        UpdateCharacterDisplay();
    }

    private void OnNextCharacterClicked()
    {
        if (characterDisplayObjects == null || characterDisplayObjects.Length == 0) return;
        SelectedCharacterIndex = (SelectedCharacterIndex + 1) % characterDisplayObjects.Length;
        UpdateCharacterDisplay();
    }

    private void UpdateCharacterDisplay()
    {
        if (characterDisplayObjects != null)
        {
            for (int i = 0; i < characterDisplayObjects.Length; i++)
                characterDisplayObjects[i].SetActive(i == SelectedCharacterIndex);
        }

        if (characterNameText != null)
        {
            characterNameText.text = (characterNames != null && SelectedCharacterIndex < characterNames.Length)
                ? characterNames[SelectedCharacterIndex]
                : $"Character {SelectedCharacterIndex + 1}";
        }
    }
    #endregion

    #region Start / Create / Join
    private async void OnHostButtonClicked()
    {
        if (runnerStarted || string.IsNullOrEmpty(PlayerName)) return;
        await StartRunner(GameMode.Host, PlayerName);
        ShowLobby(PlayerName, isHost: true);
    }

    private async void OnJoinButtonClicked()
    {
        if (runnerStarted || string.IsNullOrEmpty(PlayerName)) return;
        await StartRunner(GameMode.Client, PlayerName);
        ShowLobby(PlayerName, isHost: false);
    }

    private async Task StartRunner(GameMode mode, string sessionName)
    {
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = false;
        }

        if (!callbacksAdded)
        {
            _runner.AddCallbacks(this);
            callbacksAdded = true;
        }

        var sceneManager = _runner.GetComponent<NetworkSceneManagerDefault>()
                           ?? _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            SceneManager = sceneManager,
            PlayerCount = 8
        });

        if (!result.Ok)
        {
            Debug.LogError($"StartGame failed: {result.ShutdownReason}");
            return;
        }

        runnerStarted = true;
        
        readyStates.Clear();
        playerSelections.Clear();

        if (!_runner.LocalPlayer.IsNone)
        {
            readyStates[_runner.LocalPlayer] = false;
            playerSelections[_runner.LocalPlayer] = SelectedCharacterIndex;
        }

        if (startButton != null) startButton.gameObject.SetActive(mode == GameMode.Host);
        _runner.ProvideInput = true;
    }
    #endregion

    #region Lobby UI
    private void ShowLobby(string sessionName, bool isHost)
    {
        lobbyUI?.SetActive(true);
        menuUI?.SetActive(false);
        startButton?.gameObject.SetActive(isHost);
        readyButton?.gameObject.SetActive(!isHost);
        leaveButton?.gameObject.SetActive(true);

        RefreshPlayerList();
    }

    private void RefreshPlayerList()
    {
        if (playerListContainer == null || playerListPrefab == null || _runner == null) return;

        foreach (Transform t in playerListContainer) Destroy(t.gameObject);

        foreach (var p in _runner.ActivePlayers)
        {
            var go = Instantiate(playerListPrefab, playerListContainer);
            var text = go.GetComponentInChildren<TMP_Text>();

            bool isReady = readyStates.ContainsKey(p) && readyStates[p];
            int charIndex = playerSelections.ContainsKey(p) ? playerSelections[p] : 0;
            string charName = (characterNames != null && charIndex < characterNames.Length)
                ? characterNames[charIndex]
                : $"Char{charIndex}";

            if (text != null)
            {
                string playerLabel = $"P{p.PlayerId} - {charName} {(isReady ? "[READY]" : "")}";
                if (_runner.LocalPlayer == p) playerLabel = "(You) " + playerLabel;
                if (IsPlayerHost(p)) playerLabel += " (Host)";
                text.text = playerLabel;
            }
        }
    }

    private void OnReadyClicked()
    {
        if (_runner == null || _runner.LocalPlayer.IsNone) return;

        var local = _runner.LocalPlayer;
        bool current = readyStates.ContainsKey(local) && readyStates[local];
        readyStates[local] = !current;
        playerSelections[local] = SelectedCharacterIndex;
        RefreshPlayerList();
    }

    private async void OnLeaveClicked()
    {
        if (_runner == null) return;

        lobbyUI?.SetActive(false);
        menuUI?.SetActive(true);

        if (callbacksAdded)
        {
            _runner.RemoveCallbacks(this);
            callbacksAdded = false;
        }

        await _runner.Shutdown();
        Destroy(_runner);
        _runner = null;
        runnerStarted = false;
    }

    private async void OnStartClicked()
    {
        if (_runner == null || !_runner.IsSceneAuthority) return;

        foreach (var player in _runner.ActivePlayers)
        {
            if (IsPlayerHost(player)) continue;
            if (!readyStates.ContainsKey(player) || !readyStates[player])
            {
                Debug.Log($"Cannot start - player {player.PlayerId} not ready.");
                return;
            }
        }

        foreach (var player in _runner.ActivePlayers)
            if (!playerSelections.ContainsKey(player))
                playerSelections[player] = 0;

        var sceneRef = SceneRef.FromIndex(gameSceneBuildIndex);
        await _runner.LoadScene(sceneRef, LoadSceneMode.Single);
    }
    #endregion

    #region Helpers
    private bool IsPlayerHost(PlayerRef p)
    {
        if (_runner == null) return false;
        int min = int.MaxValue;
        PlayerRef host = default;
        foreach (var pl in _runner.ActivePlayers)
        {
            if (pl.PlayerId < min) { min = pl.PlayerId; host = pl; }
        }
        return p == host;
    }
    #endregion

    #region Overrides from NetworkRunnerCall
    public override void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        runnerStarted = false;
        if (callbacksAdded)
        {
            runner.RemoveCallbacks(this);
            callbacksAdded = false;
        }
    }

    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        readyStates[player] = false;
        playerSelections[player] = 0;
        RefreshPlayerList();
    }

    public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        readyStates.Remove(player);
        playerSelections.Remove(player);
        RefreshPlayerList();
    }

    public override void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsSceneAuthority) return;

        foreach (var player in runner.ActivePlayers)
        {
            int idx = playerSelections.ContainsKey(player) ? playerSelections[player] : 0;
            if (playerPrefabs == null || playerPrefabs.Length == 0) continue;
            if (idx < 0 || idx >= playerPrefabs.Length) idx = 0;

            var prefab = playerPrefabs[idx];
            Vector3 pos = new Vector3(player.PlayerId * 2f, 0f, 0f);

            var obj = runner.Spawn(prefab, pos, Quaternion.identity, player);
            var ctrl = obj.GetComponent<PlayerNetworkController>();
            if (ctrl != null)
            {
                ctrl.CharacterIndex = idx; // sync cho toàn bộ client
                ctrl.PlayerName = PlayerName;
            }
        }
    }

    public override void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.LogWarning($"Disconnected: {reason}");
        lobbyUI?.SetActive(false);
        menuUI?.SetActive(true);
    }
    #endregion
}
