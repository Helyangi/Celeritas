using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

// 이동 입력이 있을 때의 상태
public class PlayerMoveState : PlayerBaseMovementState
{
    private float _moveSpeed = 5;
    // FixedUpdateState에서 사용할 입력값. 같은 프레임의 UpdateState에서 이 State가 직접 읽어 저장해둔다
    private float _moveInput;

    public PlayerMoveState(PlayerController controller) : base(controller) { }

    public override void EnterState()
    {
        // Move 상태 진입 시 처리할 내용 (필요하면 이동 애니메이션 재생 등 추가)
    }

    public override void UpdateState()
    {
        // 이 State가 직접 입력을 받아서 저장 및 전환 여부를 판단한다
        _moveInput = GetMoveInput();

        // 이동 입력이 사라지면 Idle 상태로 전환
        if (_moveInput == 0f)
        {
            controller.ChangeMovementState(controller.IdleState);
        }
    }

    public override void FixedUpdateState()
    {
        // 입력 방향으로 실제 이동 처리 (2D 기준 X, Y 평면 이동)
        Vector3 moveDirection = new Vector3(_moveInput, 0, 0f).normalized;
        controller.transform.position += moveDirection * _moveSpeed * Time.fixedDeltaTime;
    }

    public override void ExitState()
    {
        // Move 상태에서 빠져나갈 때 처리할 내용
    }
}
