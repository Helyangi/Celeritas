using UnityEngine;

public class PlayerFallState : PlayerBaseMovementState
{
    public PlayerFallState(PlayerController controller) : base(controller) {}
    
    private float _fallMoveInput;
    
    public override void EnterState()
    {
        controller.Anim.Play("Fall");
    }

    public override void UpdateState()
    {
        // 상태 변경 조건 체크
        ChangeJumpState();
        ChangeWallGrabState();

        if (controller.GroundChecker.IsGround())
        {
            SoundManager.Instance.PlaySFX(SFXType.PlayerGrabLanding, controller.transform);
            controller.CanJump = true;
            controller.ChangeMovementState(controller.IdleState);
        }

        _fallMoveInput = GetMoveInput();
    }

    public override void FixedUpdateState()
    {
        Move(_fallMoveInput);
    }
    public override void ExitState()
    {
        
    }
}
