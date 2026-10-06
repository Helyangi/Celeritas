using UnityEngine;

public class PlayerWallGrabState : PlayerBaseMovementState
{
    public PlayerWallGrabState (PlayerController controller) : base(controller) { }

    // 일단 벽잡기에 대해서 자세하게 상의된 게 없어서
    // 벽에 닿은 상태에서 벽쪽으로 움직이면 안 움직이게 해뒀음
    // 미끄러지거나 하는 다른 행동 넣고 싶으면 같이 수정ㄱㄱ
    
    // 벽에 붙는 키를 따로 만든다.
    // 기본은 미끄러짐..????
    
    private float _originalGravityScale;

    public override void EnterState()
    {
        _originalGravityScale = controller.RB.gravityScale;
        controller.RB.gravityScale = 0;
        controller.RB.linearVelocity = Vector2.zero;

        controller.CanJump = true;
    }

    public override void UpdateState()
    {
        if (controller.WallChecker.GetWallDirection() * GetMoveInput() <= 0)
        {
            controller.ChangeMovementState(controller.FallState);
        }
    }

    public override void ExitState()
    {
        controller.RB.gravityScale = _originalGravityScale;
    }
}
