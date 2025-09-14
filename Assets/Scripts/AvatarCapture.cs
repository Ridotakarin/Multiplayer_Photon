using System.Collections;
using UnityEngine;

public class AvatarCapture : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject[] _players;

    [ContextMenu("Capture")]
    private void Capture()
    {
        StartCoroutine(StartCapture());
    }

    private IEnumerator StartCapture()
    { 
        foreach (var player in _players)
        {
            player.SetActive(false);
        }

        foreach (var player in _players)
        {
            string fullName = $"{player.name}.png";
            player.SetActive(true);
            yield return null;
            ScreenCapture.CaptureScreenshot(fullName);
            yield return null;
            player.SetActive(false);
        }
    }
}
