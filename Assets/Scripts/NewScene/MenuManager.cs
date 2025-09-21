using Fusion;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
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

    [Header("Build index of gameplay scene (set in Build Settings)")]
    public int gameSceneBuildIndex = 1;

    // runtime
    public static string PlayerName;
    public static int SelectedCharacterIndex = 0;

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
        startButton?.onClick.AddListener(() => _ = OnStartButtonClicked());
        leaveButton?.onClick.AddListener(OnLeaveLobbyClicked);

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
        if (startButton != null) startButton.interactable = ok;
        if (leaveButton != null) leaveButton.interactable = ok;
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
            PlayerPrefs.SetInt("SelectedCharacterIndex", SelectedCharacterIndex);
            Debug.Log($"SelectedCharacterIndex = {SelectedCharacterIndex}");
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
        PlayerPrefs.SetString("Host_PlayerName", PlayerName);
        PlayerPrefs.SetInt("Host_CharacterIndex", SelectedCharacterIndex);
        menuUI?.SetActive(false);
        lobbyUI?.SetActive(true);
        startButton?.gameObject.SetActive(true);
    }

    private async void OnJoinUIClicked()
    {
        if (!isHost)
        {
            PlayerPrefs.SetString("Client_PlayerName", PlayerName);
            PlayerPrefs.SetInt("Client_CharacterIndex", SelectedCharacterIndex);
            Debug.Log($"[Menu] Client_PlayerName={PlayerName}, Client_CharacterIndex={SelectedCharacterIndex}");
            await NetworkGameManager.Instance.StartClient("Room1", gameSceneBuildIndex);
        }
    }

    private async Task OnStartButtonClicked()
    {
        if (!isHost) return;

        bool ok = await NetworkGameManager.Instance.StartHost("Room1", gameSceneBuildIndex);
        if (!ok)
        {
            ShowMenu();
        }
    }

    private void OnLeaveLobbyClicked()
    {
        lobbyUI?.SetActive(false);
        menuUI?.SetActive(true);
        isHost = false;
    }

    private void ShowMenu()
    {
        lobbyUI?.SetActive(false);
        menuUI?.SetActive(true);
        startButton?.gameObject.SetActive(false);
    }
    #endregion
}
