using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class BattleSceneLoader : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    private int _playerCount;
    private bool isInBattle;

    public void PlayerJoined(PlayerRef player)
    {
        if (Runner == null || !Runner.IsSceneAuthority) return;
        _playerCount++;

        if (!isInBattle && _playerCount == 2)
        {
            Invoke(nameof(ChangeScene), 1f);
            isInBattle = true;
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner == null || !Runner.IsSceneAuthority) return;
        _playerCount--;
    }

    private void ChangeScene()
    { 
        print("Change scene");
        Runner.LoadScene(SceneRef.FromIndex(1));

        /* var playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (var player in playerMovements)
        {
            if (player.HasStateAuthority)
            { 
                player.LoadSceneRpc("Battle");
                return;
            }
        }
        SceneManager.LoadScene("Battle"); */
    }
}
