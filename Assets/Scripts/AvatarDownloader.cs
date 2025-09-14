using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class AvatarDownloader : MonoBehaviour
{
    [SerializeField] private TMP_InputField _avatarUrlInput;
    [SerializeField] private RawImage _avatarImage;

    public void ApplyAvatarUrl() => StartCoroutine(DownloadAvatar());

    private IEnumerator DownloadAvatar()
    {
        var url = _avatarUrlInput.text;
        var request = UnityWebRequestTexture.GetTexture(url);
        var task = request.SendWebRequest();
        yield return task;
        var handler = request.downloadHandler as DownloadHandlerTexture;
        _avatarImage.texture = handler.texture;
    }
}
