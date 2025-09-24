using Fusion;
using UnityEngine;
public struct NetworkInputData : INetworkInput
{
    public Vector2 moveDir; 
    public bool jump;
    public bool attack;
    public bool ready;
}