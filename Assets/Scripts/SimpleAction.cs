using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class SimpleAction : NetworkBehaviour
{
    [SerializeField] private NetworkMecanimAnimator _networkAnimator;

    void Update()
    {
        if (!HasStateAuthority) return;

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            _networkAnimator.SetTrigger("Attack");
        }

        _networkAnimator.Animator.SetBool("IsRunning", Keyboard.current.kKey.isPressed);
    }
}
