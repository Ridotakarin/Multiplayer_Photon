using Fusion;
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

    [SerializeField] private GameObject menuUI;
    //[SerializeField] private GameObject lobbyUI;

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
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // UI listeners
        nameInputField?.onValueChanged.AddListener(OnNameInputChanged);
        previousCharacterButton?.onClick.AddListener(OnPreviousCharacterClicked);
        nextCharacterButton?.onClick.AddListener(OnNextCharacterClicked);

        hostButton?.onClick.AddListener(OnHostUIClicked);
        joinButton?.onClick.AddListener(OnJoinUIClicked);

        // UI mặc định
        //lobbyUI?.SetActive(false);
        menuUI?.SetActive(true);
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
    }

    private void DisableInteractivity()
    {
        if (hostButton != null) hostButton.interactable = false;
        if (joinButton != null) joinButton.interactable = false;
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

            // Lưu lại lựa chọn để PlayerNetworkController đọc
            PlayerPrefs.SetInt("SelectedCharacterIndex", SelectedCharacterIndex);
            Debug.Log($"[Menu] SelectedCharacterIndex = {SelectedCharacterIndex}");
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
    private async void OnHostUIClicked()
    {
        isHost = true;
        SavePlayerPrefs();

        Debug.Log($"[Menu] Host_PlayerName={PlayerName}, CharacterIndex={SelectedCharacterIndex}");
        DisableInteractivity();

        await NetworkGameManager.Instance.StartHost("Room1", gameSceneBuildIndex);
    }

    private async void OnJoinUIClicked()
    {
        if (!isHost)
        {
            SavePlayerPrefs();

            Debug.Log($"[Menu] Client_PlayerName={PlayerName}, CharacterIndex={SelectedCharacterIndex}");

            await NetworkGameManager.Instance.StartClient("Room1", gameSceneBuildIndex);
        }
    }

    private void SavePlayerPrefs()
    {
        PlayerPrefs.SetString("PlayerName", PlayerName);
        PlayerPrefs.SetInt("SelectedCharacterIndex", SelectedCharacterIndex);
        PlayerPrefs.Save();
    }
    #endregion
}
