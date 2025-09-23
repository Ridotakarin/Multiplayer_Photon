using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;

public class GameLogic : NetworkBehaviour
{
    public static GameLogic Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Host sẽ gọi mỗi khi có thay đổi (join/leave)
    

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerNetworkController>();
        if (player != null && Runner.IsServer) // chỉ host xác định winner
        {
            Debug.Log($"{player.PlayerName} is the winner!");
            if (UIManager.Instance != null)
                UIManager.Instance.RPC_ShowWinner(player.PlayerName);
        }
    }
}
