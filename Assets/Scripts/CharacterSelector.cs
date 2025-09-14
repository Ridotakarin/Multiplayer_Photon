using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    [SerializeField] private Image _previewImage;
    [SerializeField] private CharacterDatabase _characterDb;
    [SerializeField] private PlayerConfig _playerConfig;

    private int _selectedIndex;

    private void Start() => ApplyIndex();

    public void SelectLeft()
    {
        if (_selectedIndex > 0)
        {
            _selectedIndex--;
        }
        ApplyIndex();
    }

    public void SelectRight()
    {
        if (_selectedIndex < _characterDb.CharacterInfos.Length - 1)
        {
            _selectedIndex++;
        }
        ApplyIndex();
    }

    private void ApplyIndex()
    {
        _previewImage.sprite = _characterDb.CharacterInfos[_selectedIndex].Sprite;
        _playerConfig.PrefabIndex = _selectedIndex;
    }
}
