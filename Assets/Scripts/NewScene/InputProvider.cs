using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class InputProvider : NetworkRunnerCall
{
    public PlayerNetworkController LocalPlayer;
    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
        var data = new NetworkInputData();
        var cam = Camera.main;
        Vector3 forward = cam.transform.forward; forward.y = 0; forward.Normalize();
        Vector3 right = cam.transform.right; right.y = 0; right.Normalize();

        Vector3 dir = forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal");
        data.moveDir = new Vector2(dir.x, dir.z);
        data.jump = Input.GetKey(KeyCode.Space);
        data.ready = Input.GetKey(KeyCode.R);
        data.attack = Input.GetMouseButton(0);
        input.Set(data);
    }


}
