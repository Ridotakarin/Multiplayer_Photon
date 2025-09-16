using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

namespace AvocadoShark
{
    public class NetworkGameManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        public static NetworkGameManager Instance;

        [Header("Player Prefab")]
        public NetworkPrefabRef playerPrefab;

        [Header("Spawn Settings")]
        public bool useRandomSpawn = true;
        public Vector3 customSpawn = new Vector3(0, 0, 0);

        private NetworkRunner _runner;
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
        }

        private async void Start()
        {
            // Khởi động runner (host hoặc client)
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);

            var result = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Shared,    // Hoặc GameMode.Host / Client tuỳ bạn muốn
                SessionName = "TestRoom",
                Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            });

            if (!result.Ok)
            {
                Debug.LogError($"StartGame failed: {result.ShutdownReason}");
            }
        }

        #region INetworkRunnerCallbacks

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                Debug.Log($"Player {player} joined");

                Vector3 spawnPos = useRandomSpawn
                    ? new Vector3(Random.Range(-5f, 5f), 1, Random.Range(-5f, 5f))
                    : customSpawn;

                NetworkObject playerObj = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
                _spawnedPlayers.Add(player, playerObj);
            }
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (_spawnedPlayers.TryGetValue(player, out var obj))
            {
                runner.Despawn(obj);
                _spawnedPlayers.Remove(player);
            }
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            // Gửi input từ bàn phím/chuột
            var data = new NetworkInputData();

            data.direction.x = Input.GetAxis("Horizontal");
            data.direction.z = Input.GetAxis("Vertical");

            if (Input.GetKey(KeyCode.Space))
                data.buttons |= NetworkInputData.JUMP;

            input.Set(data);
        }

        // Các hàm interface bắt buộc nhưng chưa dùng
        public void OnConnectedToServer(NetworkRunner runner) { }
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
        public void OnSceneLoadStart(NetworkRunner runner) { }
        public void OnSceneLoadDone(NetworkRunner runner) { }
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        #endregion
    }

    /// <summary>
    /// Input data gửi qua mạng
    /// </summary>
    public struct NetworkInputData : INetworkInput
    {
        public const byte JUMP = 0x01;

        public Vector3 direction;
        public byte buttons;

        public bool IsJumpPressed => (buttons & JUMP) != 0;
    }
}
