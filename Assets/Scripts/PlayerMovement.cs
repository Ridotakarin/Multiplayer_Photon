using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private float _speed;

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        var input = _moveAction.action.ReadValue<Vector2>();
        var direction = Vector3.forward * input.y + Vector3.right * input.x;
        transform.Translate(_speed * Runner.DeltaTime * direction);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.Proxies)]
    public void LoadSceneRpc(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
