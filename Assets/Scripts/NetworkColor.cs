using Fusion;
using UnityEngine;

public class NetworkColor : NetworkBehaviour
{
    [SerializeField] private Renderer _renderer;

    [Networked, OnChangedRender(nameof(ApplyColor))]
    public Color SkinColor { get; set; }

    public override void Spawned()
    {
        base.Spawned();
        ApplyColor();
    }

    private void ChangeColor() => SkinColor = Random.ColorHSV();

    private void ApplyColor() => _renderer.material.color = SkinColor;
}
