using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _camera;

    private void Update()
    {
        if (!_camera)
        {
            _camera = Camera.main.transform;
        }
        transform.forward = _camera.forward;
    }
}
