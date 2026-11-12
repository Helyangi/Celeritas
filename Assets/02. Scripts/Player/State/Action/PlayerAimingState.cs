using System.Collections.Generic;
using UnityEngine;

public class PlayerAimingState : PlayerBaseActionState
{
    public PlayerAimingState(PlayerController controller) : base(controller) {}    
    private Vector2 _direction;
    private bool _canLightMove;    // 직전 프레임의 조준선이 빛 구체에 닿아 있었는가
    private RaycastHit2D _lastHit; // 직전 프레임 조준선의 마지막 hit (버튼을 뗄 때 발사 판정에 사용)
    private bool _isFired;         // 이번 조준이 발사로 끝났는가

    public override void EnterState()
    {
        _canLightMove = false;
        _lastHit = default;
        _isFired = false;
        controller.LightAimingLine.SetAiming(true);
    }
    public override void UpdateState()
    {
        // 버튼을 뗐다면 이번 프레임에는 조준선을 다시 계산하지 않는다.
        // 뗄 때 손이 흔들려도 "마지막으로 보고 있던 조준선" 그대로 발사 여부를 정하기 위해서다.
        if (!GetAimHeld())
        {
            ReleaseAim();
            return;
        }

        Vector2 myPos = controller.Light.transform.position;
        _direction = (GetMousePos() - myPos).normalized; // 1. 마우스 방향을 먼저 계산

        _lastHit = controller.LightAimingLine.CalculateLightPath(_direction); // 2. 반사까지 포함한 경로 계산 (마지막으로 닿은 hit 반환)
        controller.LightAimingLine.DrawAimingLine(); // 3. 계산된 점들로 랜더링
        _canLightMove = controller.LightAimingLine.HandleHitLayer(_lastHit); // 4. 마지막으로 닿은 게 뭔지 판단
        controller.LightAimingLine.SettingLightSensor();
        controller.SetAimingAtOrb(_canLightMove); // 튜토리얼 등이 구독할 수 있게 조준선이 오브에 닿았는지 알린다
    }
    public override void ExitState()
    {
        // 발사로 끝난 경우에는 빛 이동 동안 구체의 발광을 유지하고(도착하면 LightMoveState가 끈다), 취소로 끝난 경우에는 바로 끈다.
        if (!_isFired) controller.LightAimingLine.ClearLightSensors();
        controller.SetAimingAtOrb(false);
    }
    private void ReleaseAim()
    {
        if (_canLightMove && _lastHit.collider != null)
        {
            _isFired = true;
            controller.LightAimingLine.SaveLightPath(_lastHit, controller.LightAimingLine.LightPath); // 5. 이동 경로를 컨트롤러에 저장하고
            controller.ChangeMovementState(controller.LightMoveState); // 빛 이동 상태로 전환 (이때 행동 상태는 NoneState가 된다)
        }
        else
        {
            controller.ChangeActionState(controller.NoneState); // 빛 구체에 안 닿은 채로 뗐다면 발사하지 않고 취소
        }
    }
}
