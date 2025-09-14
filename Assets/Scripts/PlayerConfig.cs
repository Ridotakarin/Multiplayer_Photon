using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Scriptable Objects/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    public string PlayerName { get; set; } = "No name";
    public Color Color { get; set; } = Color.white;
    public int PrefabIndex { get; set; }
}
