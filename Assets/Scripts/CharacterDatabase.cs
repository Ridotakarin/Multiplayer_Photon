using UnityEngine;

[System.Serializable]
public struct CharacterInfo
{
    public string AddressableKey;
    public string DisplayName;
    public Sprite Sprite;
}

[CreateAssetMenu(fileName = "CharacterDb", menuName = "Scriptable Objects/CharacterDb")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField] private CharacterInfo[] _characterInfos;

    public CharacterInfo[] CharacterInfos => _characterInfos;
}
