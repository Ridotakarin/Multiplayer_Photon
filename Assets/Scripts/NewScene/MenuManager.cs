using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Threading.Tasks;

public class MenuManager : NetworkRunnerCall
{
    public static MenuManager Instance;

    [Header("UI Components")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button startButton;
    [SerializeField] private Button leaveButton;
    [SerializeField] private GameObject menuUI;
    [SerializeField] private GameObject lobbyUI;

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
    private bool isHost = false;

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

        hostButton?.onClick.AddListener(OnHostUIClicked);
        joinButton?.onClick.AddListener(OnJoinUIClicked);
        startButton?.onClick.AddListener(OnStartButtonClicked);
        leaveButton?.onClick.AddListener(OnLeaveLobbyClicked);

        _runner = gameObject.GetComponent<NetworkRunner>();
        if (_runner != null) _runner.ProvideInput = false;

        // UI mặc định
        lobbyUI?.SetActive(false);
        menuUI?.SetActive(true);
        startButton?.gameObject.SetActive(false);
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

    #region Lobby Flow
    private void OnHostUIClicked()
    {
        isHost = true;
        menuUI?.SetActive(false);
        lobbyUI?.SetActive(true);
        startButton?.gameObject.SetActive(true);
    }

    private void OnJoinUIClicked()
    {
        isHost = false;
        menuUI?.SetActive(false);
        lobbyUI?.SetActive(true);
        startButton?.gameObject.SetActive(false);
        // client sẽ chờ host start game => chưa start runner ở đây
    }

    private async void OnStartButtonClicked()
    {
        if (!isHost || runnerStarted) return;

        await StartRunner(GameMode.Host, "Room1");

        if (_runner != null)
        {
            var sceneRef = SceneRef.FromIndex(gameSceneBuildIndex);
            await _runner.LoadScene(sceneRef, LoadSceneMode.Single);
        }
    }

    private void OnLeaveLobbyClicked()
    {
        // quay về menu, chưa start runner thì chỉ cần UI toggle
        lobbyUI?.SetActive(false);
        menuUI?.SetActive(true);
        isHost = false;
    }
    #endregion

    #region Runner
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

        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 8
        });

        if (!result.Ok)
        {
            Debug.LogError($"Failed to start: {result.ShutdownReason}");
            await _runner.Shutdown();
            Destroy(_runner);
            _runner = null;
            runnerStarted = false;
            callbacksAdded = false;
            return;
        }

        runnerStarted = true;
        _runner.ProvideInput = true;
    }
    #endregion

    #region Overrides
    public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer && !runner.TryGetPlayerObject(player, out var playerObj))
        {
            int idx = (player == runner.LocalPlayer) ? SelectedCharacterIndex : 0;
            if (idx < 0 || idx >= playerPrefabs.Length) idx = 0;

            Vector3 pos = new Vector3(player.PlayerId * 2f, 0f, 0f);
            playerObj = runner.Spawn(playerPrefabs[idx], pos, Quaternion.identity, player);

            var ctrl = playerObj.GetComponent<PlayerNetworkController>();
            if (ctrl != null)
            {
                ctrl.CharacterIndex = idx;
                ctrl.PlayerName = (player == runner.LocalPlayer) ? PlayerName : $"Player {player.PlayerId}";
            }

            Debug.Log($"[OnPlayerJoined] Spawn Player {player.PlayerId}, Name={ctrl.PlayerName}, CharIndex={ctrl.CharacterIndex}");
        }
    }

    public override void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("[SceneLoadDone] Scene loaded xong, player objects đã migrate.");
    }
    #endregion
}
