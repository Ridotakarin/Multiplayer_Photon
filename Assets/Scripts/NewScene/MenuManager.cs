using AvocadoShark;
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

    // Các biến cho việc chọn nhân vật
    [Header("Character Selection UI")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Button previousCharacterButton;
    [SerializeField] private Button nextCharacterButton;

    // Danh sách các GameObject của nhân vật để hiển thị trong menu
    [Header("Character Display Objects")]
    public GameObject[] characterDisplayObjects;

    // Tên của các nhân vật
    [Header("Character Names")]
    public string[] characterNames;

    // Prefabs mạng cho 4 nhân vật (không thay đổi)
    [Header("Network Prefabs")]
    public NetworkPrefabRef[] playerPrefabs;

    public static string PlayerName;
    public static int SelectedCharacterIndex = -1;

    [SerializeField]private NetworkRunner _runner;

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

        hostButton.interactable = false;
        joinButton.interactable = false;
        // Thêm listener cho InputField
        nameInputField.onValueChanged.AddListener(OnNameInputChanged);

        // Thêm listener cho các nút
        hostButton.onClick.AddListener(OnHostButtonClicked);
        joinButton.onClick.AddListener(OnJoinButtonClicked);
        previousCharacterButton.onClick.AddListener(SelectPreviousCharacter);
        nextCharacterButton.onClick.AddListener(SelectNextCharacter);
        _runner = FindObjectOfType<NetworkRunner>();
    }


    private void Start()
    {
        // Khởi tạo lựa chọn ban đầu
        SelectedCharacterIndex = 0;
        UpdateCharacterSelectionUI();
        CheckButtonsInteractability();
    }

    private void OnNameInputChanged(string name)
    {
        PlayerName = name;
        CheckButtonsInteractability();
    }

    private void SelectPreviousCharacter()
    {
        
        if (SelectedCharacterIndex > 0)
        {
            SelectedCharacterIndex--;
        }
        else
        {
            SelectedCharacterIndex = characterDisplayObjects.Length - 1;
        }
        UpdateCharacterSelectionUI();
    }

    private void SelectNextCharacter()
    {
        if (SelectedCharacterIndex < characterDisplayObjects.Length - 1)
        {
            SelectedCharacterIndex++;
        }
        else
        {
            SelectedCharacterIndex = 0;
        }
        UpdateCharacterSelectionUI();
    }

    private void UpdateCharacterSelectionUI()
    {
        // Duyệt qua tất cả các đối tượng và chỉ bật cái được chọn
        for (int i = 0; i < characterDisplayObjects.Length; i++)
        {
            characterDisplayObjects[i].SetActive(i == SelectedCharacterIndex);
            Debug.Log("Number: " + SelectedCharacterIndex);

        }

        // Cập nhật tên
        characterNameText.text = characterNames[SelectedCharacterIndex];
    }

    private void CheckButtonsInteractability()
    {
        bool hasName = !string.IsNullOrEmpty(PlayerName);
        hostButton.interactable = hasName;
        joinButton.interactable = hasName;
    }

    public async void OnHostButtonClicked()
    {
        if (_runner != null)
        {
            await StartGame(GameMode.Shared, "TestRoom");
        }
    }

    public async void OnJoinButtonClicked()
    {
        if (_runner != null)
        {
            await StartGame(GameMode.Shared, "TestRoom");
        }
    }

    private async Task StartGame(GameMode mode, string sessionName)
    {
        _runner.AddCallbacks(NetworkGameManager.Instance);
        var result = await _runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(1), // Index của scene Lobby
            SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (!result.Ok)
        {
            Debug.LogError($"Failed to start game: {result.ShutdownReason}");
        }

    }

}