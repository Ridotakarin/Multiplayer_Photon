using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    [SerializeField] private GameObject _overviewCamera;
    [SerializeField] private PlayerConfig _playerConfig;
    [SerializeField] private CharacterDatabase _characterDatabase;
    [SerializeField] private Transform[] _spawnPoints;

    private NetworkObject _newPlayer;

    public void PlayerJoined(PlayerRef newPlayerInfo)
    {
        if (newPlayerInfo == Runner.LocalPlayer)
        {
            _overviewCamera.SetActive(false);
            StartCoroutine(SpawnPlayer(newPlayerInfo));
        }
    }

    private IEnumerator SpawnPlayer(PlayerRef newPlayerInfo)
    {
        var index = (newPlayerInfo.AsIndex - 1) % _spawnPoints.Length;
        var spawnPoint = _spawnPoints[index];
        var addressableKey = _characterDatabase.CharacterInfos[_playerConfig.PrefabIndex].AddressableKey;
        var task = Addressables.LoadAssetAsync<GameObject>(addressableKey);
        yield return task;
        var playerPrefab = task.Result;
        _newPlayer = Runner.Spawn(playerPrefab, spawnPoint.position,
            spawnPoint.rotation, newPlayerInfo);
        yield return null;
        ApplyPlayerConfig();
    }

    private void ApplyPlayerConfig()
    {
        if (_newPlayer.TryGetComponent<NetworkColor>(out var networkColor))
        {
            networkColor.SkinColor = _playerConfig.Color;
        }
        
        _newPlayer.GetComponentInChildren<NetworkText>().Text = _playerConfig.PlayerName;
    }
}
