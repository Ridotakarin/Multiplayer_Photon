using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class InputProvider : NetworkRunnerCall
{
    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        data.horizontal = Input.GetAxisRaw("Horizontal");
        data.vertical = Input.GetAxisRaw("Vertical");
        data.jump = Input.GetKey(KeyCode.Space);
        input.Set(data);
    }

    
}
