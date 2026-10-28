using UnityEngine;

public class PlayerWallGrabState : PlayerBaseMovementState
{
    public PlayerWallGrabState (PlayerController controller) : base(controller) { }
    
    // 벽에 붙는 키를 따로 만든다.
    // 기본은 미끄러짐..????
    
    private float _originalGravityScale;
    private Vector3 _originLightPos;

    public override void EnterState()
    {
        _originLightPos = controller.Light.transform.localPosition;
        controller.Light.transform.position += new Vector3(-controller.WallChecker.GetWallDirection() * 0.05f, 0, 0);
        controller.Anim.Play("WallGrab");
        controller.CanJump = true;
        SoundManager.Instance.PlaySFX(SFXType.PlayerGrabLanding, controller.transform.position);
        _originalGravityScale = controller.RB.gravityScale;
        controller.RB.gravityScale = 0;
        controller.RB.linearVelocity = Vector2.zero;
    }

    public override void UpdateState()
    {
        ChangeWallJumpState();
        
        if (controller.WallChecker.GetWallDirection() * GetMoveInput() <= 0)
        {
            controller.ChangeMovementState(controller.FallState);
        }
    }

    public override void ExitState()
    {
        controller.RB.gravityScale = _originalGravityScale;
        controller.Light.transform.localPosition = _originLightPos;
    }
}
