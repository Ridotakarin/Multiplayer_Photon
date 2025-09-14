using UnityEngine;
using Fusion;
using TMPro;

public class NetworkText : NetworkBehaviour
{
    [SerializeField] private TMP_Text _tmpText;

    [Networked, OnChangedRender(nameof(ApplyNewText))]
    public string Text { get; set; }

    public override void Spawned()
    {
        base.Spawned();
        ApplyNewText();
    }

    private void ApplyNewText() => _tmpText.text = Text;
}
