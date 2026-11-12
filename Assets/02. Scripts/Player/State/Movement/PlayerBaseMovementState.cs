using UnityEngine;
public abstract class PlayerBaseMovementState : PlayerBaseState
{
    public PlayerBaseMovementState(PlayerController controller) : base(controller) {}

    // 상태 전환 함수들은 전환했으면 true를 반환한다. 호출한 UpdateState는 true면 바로 return해야 같은 프레임에 다른 전환이 덮어쓰지 않는다
    protected virtual bool ChangeJumpState() // 점프 상태 전환
    {
        if (GetJumpInput() && controller.CanJump)
        {
            controller.ChangeMovementState(controller.JumpState);
            return true;
        }
        return false;
    }

    protected virtual bool ChangeFallState()  // 낙하 상태 전환
    {
        if (controller.RB.linearVelocityY < 0 && !controller.GroundChecker.IsGround())
        {
            controller.ChangeMovementState(controller.FallState);
            return true;
        }
        return false;
    }
    
    protected virtual bool ChangeWallGrabState()  // 벽잡기 상태 전환
    {
        if (controller.WallChecker.GetWallDirection() * GetMoveInput() > 0)
        {
            controller.ChangeMovementState(controller.WallGrabState);
            return true;
        }
        return false;
    }

    protected virtual bool ChangeWallJumpState()  // 벽점프 상태 전환
    {
        if (GetJumpInput())
        {
            controller.ChangeMovementState(controller.WallJumpState);
            return true;
        }
        return false;
    }

    protected void Move(float inputMove) // 이동 함수
    {
        controller.RB.linearVelocityX = inputMove * controller.Stats.MoveSpeed;
        if (inputMove > 0)
        {
            controller.Renderer.transform.rotation = Quaternion.identity;
        }
        else if (inputMove < 0)
        {
            controller.Renderer.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }
    protected void Jump(Vector2 jumpDirection) // 점프 함수
    {
        InputManager.Instance.IsJump = false; // 입력 취소
        controller.Anim.Play("Jump");
        SoundManager.Instance.PlaySFX(SFXType.PlayerJump, controller.transform.position);
        controller.RB.linearVelocityY = 0;
        controller.RB.AddForce(jumpDirection * controller.Stats.JumpForce, ForceMode2D.Impulse);
    }
}
