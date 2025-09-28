using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Winner UI")]
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TMP_Text winnerText;

    [Header("Countdown UI")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TMP_Text countdownText;

    [Header("Button UI")]
    [SerializeField] private Button leaveButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        leaveButton?.onClick.AddListener(OnLeaveButtonClicked);
    }

    private void Start()
    {
        

        winnerPanel.SetActive(false);
        countdownPanel.SetActive(false);
    }

    public async void OnLeaveButtonClicked()
    {
        if (NetworkGameManager.Instance != null && NetworkGameManager.Instance.Runner != null)
        {
            Debug.Log("[UIManager] Leave game...");
            await NetworkGameManager.Instance.Runner.Shutdown();
        }
    }

    // === Winner UI ===
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowWinner(string winnerName)
    {
        winnerPanel.SetActive(true);
        winnerText.text = $"Winner: {winnerName}";
    }

    // === Countdown UI ===
    public void ShowCountdown(bool show, float value = 0)
    {
        countdownPanel.SetActive(show);
        if (show)
        {
            countdownText.text = Mathf.CeilToInt(value).ToString();
        }
    }
}
