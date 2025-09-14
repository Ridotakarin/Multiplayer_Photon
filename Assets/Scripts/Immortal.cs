using UnityEngine;

public class Immortal : MonoBehaviour
{
    private void Awake() => DontDestroyOnLoad(gameObject);
}
