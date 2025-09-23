using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class InputProvider : NetworkRunnerCall
{
    public PlayerNetworkController LocalPlayer;
    public override void OnInput(NetworkRunner runner, NetworkInput input)
    {
        
        var data = new NetworkInputData();
        data.horizontal = Input.GetAxisRaw("Horizontal");
        data.vertical = Input.GetAxisRaw("Vertical");
        data.jump = Input.GetKey(KeyCode.Space);
        data.ready = Input.GetKey(KeyCode.R);
        input.Set(data);
    }


}
