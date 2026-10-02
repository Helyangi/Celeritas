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
        // 상태 변경 조건 체크
        ChangeFallState();
        
        _jumpMoveInput = GetMoveInput();
    }

    public override void FixedUpdateState()
    {
        Move(_jumpMoveInput);
    }
}