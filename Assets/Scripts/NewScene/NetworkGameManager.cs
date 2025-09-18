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

        [Networked]
        public bool IsGameStarted { get; set; } = false;

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
            _runner = GameObject.FindAnyObjectByType<NetworkRunner>();

            // Nếu không tìm thấy, tạo mới từ prefab
            if (_runner == null)
            {
                _runner = Instantiate(runnerPrefab);
                _runner.name = "NetworkRunner";
                _runner.ProvideInput = true;
                _runner.AddCallbacks(this);
            }
        }


        #region INetworkRunnerCallbacks

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                // Chọn prefab dựa trên lựa chọn của người chơi
                var selectedIndex = MenuManager.SelectedCharacterIndex;
                var selectedPrefab = playerPrefabs[selectedIndex];

                // Lấy một điểm spawn từ hàng đợi
                Transform spawnPoint = availableSpawnPoints.Dequeue();
                Vector3 spawnPos = spawnPoint.position;
                Quaternion spawnRot = spawnPoint.rotation;

                // Spawn player prefab tại vị trí chờ
                NetworkObject playerObj = runner.Spawn(selectedPrefab, spawnPos, spawnRot, player);
                runner.SetPlayerObject(player, playerObj);

                // Gán playerController và lưu điểm spawn
                var playerController = playerObj.GetComponent<PlayerNetworkController>();
                playerController.SetInitialSpawnPoint(spawnPoint.position, spawnPoint.rotation);

                _spawnedPlayers[player] = playerObj;

                // Cập nhật thông tin người chơi trên mạng
                PlayerNames.Add(player, MenuManager.PlayerName);
                PlayerCharacterSelections.Add(player, selectedIndex);

                Debug.Log($">>> Spawned {playerObj.name} at waiting area for {player}");
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