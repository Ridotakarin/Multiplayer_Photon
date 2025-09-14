using UnityEngine;
using Fusion;

public class NetworkActivator : NetworkBehaviour
{
    [SerializeField] private GameObject[] _ownerObjects;

    private void Start()
    {
        var isActive = HasStateAuthority;
        foreach (var obj in _ownerObjects)
        {
            obj.SetActive(isActive);
        }
    }
}
