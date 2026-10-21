using UnityEngine;

public class PlayerAnimationEventReceiver : MonoBehaviour
{
    [SerializeField] private Transform _footTransform;
    public void PlayFootstepSound()
    {
        SoundManager.Instance.PlaySFX(SFXType.PlayerWalkstep, _footTransform.position);
    }
}
