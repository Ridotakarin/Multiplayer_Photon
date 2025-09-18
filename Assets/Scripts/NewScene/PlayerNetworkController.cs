using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(NetworkCharacterController))]
public class PlayerNetworkController : NetworkBehaviour
{
    private NetworkCharacterController _controller;

    [SerializeField] private float moveSpeed = 5f;

    // Thêm các biến networked để lưu vị trí spawn ban đầu
    [Networked] public Vector3 InitialSpawnPosition { get; set; }
    [Networked] public Quaternion InitialSpawnRotation { get; set; }

    public override void Spawned()
    {
        _controller = GetComponent<NetworkCharacterController>();

        if (Object.HasInputAuthority)
            GetComponentInChildren<Renderer>().material.color = Color.blue;
        else
            GetComponentInChildren<Renderer>().material.color = Color.red;

        // Kiểm tra nếu là player đầu tiên được spawn và có quyền input
        // Cần đảm bảo rằng vị trí này được set bởi server
        if (Object.HasStateAuthority)
        {
            // Trong trường hợp này, việc này sẽ được set trong NetworkGameManager
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            Vector3 move = new Vector3(input.horizontal, 0, input.vertical);
            _controller.Move(move.normalized * moveSpeed);

            if (move != Vector3.zero)
                transform.forward = move.normalized;

            if (input.jump)
                _controller.Jump();
        }
    }

    // Hàm này chỉ được gọi từ máy chủ (Server)
    public void SetInitialSpawnPoint(Vector3 position, Quaternion rotation)
    {
        if (Object.HasStateAuthority)
        {
            InitialSpawnPosition = position;
            InitialSpawnRotation = rotation;
            transform.position = InitialSpawnPosition;
            transform.rotation = InitialSpawnRotation;
        }
    }

    // Hàm hồi sinh (Respawn)
    public void Respawn()
    {
        transform.position = InitialSpawnPosition;
        transform.rotation = InitialSpawnRotation;
    }
}