using UnityEngine;

public class PlayerJumpState : PlayerBaseMovementState
{
    // TODO: 공중 점프 규칙 확정 후 구현
    // - 최대 점프 횟수
    // - 낙하 후 첫 점프 처리
    // - 착지 시 횟수 초기화

    public PlayerJumpState(PlayerController controller) : base(controller) {}

    private float _moveSpeed = 5;
    private float _jumpForce = 10;
    private float _jumpMoveInput;
    
    public override void EnterState()
    {
        Jump();
    }

    public override void UpdateState()
    {
        _jumpMoveInput = GetMoveInput();
    }

    public override void FixedUpdateState()
    {
        Vector2 velocity = controller.RB.linearVelocity;
        velocity.x = _jumpMoveInput * _moveSpeed;
        controller.RB.linearVelocity = velocity;
    }

    private void Jump()
    {
        Vector2 velocity = controller.RB.linearVelocity;
        velocity.y = 0f;
        controller.RB.linearVelocity = velocity;

        controller.RB.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
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
