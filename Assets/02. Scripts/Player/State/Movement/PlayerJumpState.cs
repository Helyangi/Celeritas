using UnityEngine;

public class PlayerJumpState : PlayerBaseMovementState
{
    // TODO: 공중 점프 규칙 확정 후 구현
    // - 최대 점프 횟수 - 1번
    // - 낙하 후 첫 점프 처리 - 오버랩
    // - 착지 시 횟수 초기화 - 1번

    public PlayerJumpState(PlayerController controller) : base(controller) {}

    private float _jumpMoveInput;
    
    public override void EnterState()
    {
        Jump(Vector2.up);
    }

    public override void UpdateState()
    {
        _jumpMoveInput = GetMoveInput();
    }

    public override void FixedUpdateState()
    {
        Move(_jumpMoveInput);
    }

    // 지금은 그냥 땅에 충돌했는가만 보고 있어서 착지를 판정하려면 고쳐야 할듯
    public override void OnCollisionEnter2DState(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            if (_jumpMoveInput != 0)
            {
                controller.ChangeMovementState(controller.MoveState);
                return;
            }
            controller.ChangeMovementState(controller.IdleState);
        }
    }
}