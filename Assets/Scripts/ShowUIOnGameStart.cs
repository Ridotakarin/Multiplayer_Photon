// 8/26/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

using UnityEngine;

public class ShowUIOnGameStart : MonoBehaviour
{
    [SerializeField]
    private GameObject uiPanel; // Reference to the UI GameObject to be shown

    private void Start()
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(true); // Show the UI GameObject when the game starts
        }
        else
        {
            Debug.LogWarning("UI Panel reference is not assigned in the inspector.");
        }
    }
}