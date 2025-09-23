using Fusion;

public struct NetworkInputData : INetworkInput
{
    public float horizontal;
    public float vertical;
    public NetworkBool jump;
    public bool ready;
}