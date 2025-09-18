using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

namespace AvocadoShark
{
    public class NetworkGameManager : NetworkRunnerCall
    {
        public static NetworkGameManager Instance;

        [Header("Player Prefab")]
        public NetworkPrefabRef[] playerPrefabs;

        [Networked, Capacity(4)]
        public NetworkDictionary<PlayerRef, string> PlayerNames => default;
        [Networked, Capacity(4)]
        public NetworkDictionary<PlayerRef, int> PlayerCharacterSelections => default;


        [Header("Runner Prefab")]
        [SerializeField] private NetworkRunner runnerPrefab;

        [Header("Spawn Settings")]
        public Transform[] spawnPoints;
        private Queue<Transform> availableSpawnPoints;

        [SerializeField]private NetworkRunner _runner;
        private Dictionary<PlayerRef, NetworkObject> _spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

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
            _runner = FindObjectOfType<NetworkRunner>();
        }

        private void InitializeSpawnPoints()
        {
            availableSpawnPoints = new Queue<Transform>();
            Shuffle(spawnPoints);
            foreach (var point in spawnPoints)
            {
                availableSpawnPoints.Enqueue(point);
            }
        }

        private void Shuffle<T>(T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                T temp = array[i];
                array[i] = array[rnd];
                array[rnd] = temp;
            }
        }

        #region INetworkRunnerCallbacks

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log($">>> OnPlayerJoined: {player}, Local={runner.LocalPlayer}");

            if (runner.IsServer)
            {
                // Lấy prefab nhân vật đã chọn từ MenuManager
                NetworkPrefabRef selectedPrefab = playerPrefabs[MenuManager.SelectedCharacterIndex];

                // Lấy vị trí spawn cố định cho Lobby
                Vector3 lobbySpawnPos = new Vector3(0, 1, 0);
                Quaternion lobbySpawnRot = Quaternion.identity;

                // Spawn player prefab
                NetworkObject playerObj = runner.Spawn(selectedPrefab, lobbySpawnPos, lobbySpawnRot, player);
                runner.SetPlayerObject(player, playerObj);

                _spawnedPlayers[player] = playerObj;

                // Cập nhật thông tin người chơi trên mạng (tên và lựa chọn nhân vật)
                PlayerNames.Add(player, MenuManager.PlayerName);
                PlayerCharacterSelections.Add(player, MenuManager.SelectedCharacterIndex);

                Debug.Log($">>> Spawned {playerObj.name} for {player}, HasInputAuthority={playerObj.HasInputAuthority}");
            }
        }

        public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedPlayers.TryGetValue(player, out var obj))
            {
                runner.Despawn(obj);
                _spawnedPlayers.Remove(player);
            }
        }

        public override void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var data = new NetworkInputData();
            data.horizontal = Input.GetAxisRaw("Horizontal");
            data.vertical = Input.GetAxisRaw("Vertical");
            data.jump = Input.GetKey(KeyCode.Space);
            input.Set(data);
        }
        #endregion
    }
}