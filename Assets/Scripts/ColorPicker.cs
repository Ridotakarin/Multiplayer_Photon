using UnityEngine;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour
{
    [SerializeField] private PlayerConfig _playerConfig;
    [SerializeField] private Image _image;

    public void PickRandomColor()
    {
        _playerConfig.Color = Random.ColorHSV();
        _image.color = _playerConfig.Color;
    }
}
