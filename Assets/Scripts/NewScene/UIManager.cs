using Fusion;
using TMPro;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TMP_Text winnerText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public async void OnLeaveButtonClicked()
    {
        if (Runner != null)
        {
            await Runner.Shutdown();
            // có thể load lại scene menu ở đây
            UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
        }
    }
    // RPC do Host gọi khi có thay đổi

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_ShowWinner(string winnerName)
    {
        winnerPanel.SetActive(true);
        winnerText.text = $"Winner: {winnerName}";
    }
}
