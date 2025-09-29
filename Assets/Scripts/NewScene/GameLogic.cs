using System.Collections;
using Fusion;
using UnityEngine;

public enum GameState
{
    Waiting,
    Countdown,
    Playing,
    Ended
}

public class GameLogic : NetworkBehaviour
{
    public static GameLogic Instance { get; private set; }

    [Header("Gameplay Settings")]
    [SerializeField] private float countdownDuration = 15f;
    [SerializeField] private float restartDelay = 5f;
    [SerializeField] private Transform[] spawnPoints;

    // ================= Networked =================
    [Networked] public float CountdownTimer { get; private set; }
    [Networked] private NetworkBool IsCountdownActive { get; set; }
    [Networked] private GameState CurrentState { get; set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CurrentState = GameState.Waiting;

            // Bắt đầu đếm ngược ngay khi host tạo phòng
            StartCountdown();
        }
    }

    // ====== Host gọi khi có player join ======
    public void OnPlayerJoined(PlayerNetworkController player)
    {
        if (!Runner.IsServer) return;

        if (CurrentState == GameState.Waiting || CurrentState == GameState.Countdown)
        {
            ResetCountdown();
        }
    }
    public void OnPlayerLeft(PlayerRef playerRef)
    {
        if (!Runner.IsServer) return;

        if (CurrentState == GameState.Waiting || CurrentState == GameState.Countdown)
        {
            ResetCountdown();
        }
    }
    private void ResetCountdown()
    {
        if (!Runner.IsServer) return;
        Debug.Log("[GameLogic] Resetting countdown...");
        IsCountdownActive = true;
        CountdownTimer = countdownDuration;
        CurrentState = GameState.Countdown;
        // Bật panel UI cho tất cả
        RPC_ShowCountdown(true);
    }

    // ====== Countdown ======
    private void StartCountdown()
    {
        if (!Runner.IsServer) return;

        Debug.Log("[GameLogic] Starting countdown...");
        IsCountdownActive = true;
        CountdownTimer = countdownDuration;
        CurrentState = GameState.Countdown;

        // Bật panel UI cho tất cả
        RPC_ShowCountdown(true);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;

        if (IsCountdownActive)
        {
            CountdownTimer -= Runner.DeltaTime;

            if (CountdownTimer <= 0f)
            {
                CountdownTimer = 0f;
                IsCountdownActive = false;
                StartGame();
            }
        }
    }

    // ====== Start Game ======
    private void StartGame()
    {
        CurrentState = GameState.Playing;

        // Respawn tất cả player vào spawn points
        var players = FindObjectsOfType<PlayerNetworkController>();
        for (int i = 0; i < players.Length; i++)
        {
            var p = players[i];
            Vector3 pos = spawnPoints[i % spawnPoints.Length].position;
            Quaternion rot = spawnPoints[i % spawnPoints.Length].rotation;
            p.SetInitialSpawnPoint(pos, rot);
            p.Respawn();
        }

        Debug.Log("[GameLogic] Game started!");
        RPC_ShowCountdown(false);
    }

    // ====== End Game ======
    public void EndGame(PlayerNetworkController winner)
    {
        if (!Runner.IsServer) return;

        CurrentState = GameState.Ended;
        Debug.Log($"{winner.PlayerName} is the winner!");

        RPC_ShowWinner(winner.PlayerName);

        // Restart sau delay
        Runner.StartCoroutine(RestartGameCoroutine());
    }

    private IEnumerator RestartGameCoroutine()
    {
        yield return new WaitForSeconds(restartDelay);

        // Reset lại tất cả player
        var players = FindObjectsOfType<PlayerNetworkController>();
        foreach (var p in players)
        {
            p.Respawn();
        }

        StartCountdown();
    }

    // ====== RPC cho UI ======
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowWinner(string winnerName)
    {
        UIManager.Instance?.RPC_ShowWinner(winnerName);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowCountdown(bool show)
    {
        UIManager.Instance?.ShowCountdown(show, CountdownTimer);
    }

    private void Update()
    {
        // Client nào cũng update UI theo CountdownTimer
        if (IsCountdownActive && UIManager.Instance != null)
        {
            UIManager.Instance.ShowCountdown(true, CountdownTimer);
        }
    }
}
