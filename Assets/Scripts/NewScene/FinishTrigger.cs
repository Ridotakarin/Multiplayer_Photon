using Fusion;
using UnityEngine;

public class FinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Kiểm tra nếu va chạm là Player
        PlayerNetworkController player = other.GetComponent<PlayerNetworkController>();
        if (player == null) return;

        // Chỉ host mới xử lý end game
        if (GameLogic.Instance != null && GameLogic.Instance.Object.HasStateAuthority)
        {
            if (GameLogic.Instance.CurrentState == GameState.Playing)
            {
                Debug.Log($"[FinishTrigger] Player {player.PlayerName} đã chạm đích!");
                GameLogic.Instance.EndGame(player);
            }
        }
    }
}
