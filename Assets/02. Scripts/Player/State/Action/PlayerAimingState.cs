using System.Collections.Generic;
using UnityEngine;

public class PlayerAimingState : PlayerBaseActionState
{
    public PlayerAimingState(PlayerController controller) : base(controller) {}    
    private Vector2 _direction;
    private bool _canLightMove;
    public override void UpdateState()
    {
        Vector2 myPos = controller.RB.position;
        _direction = (GetMousePos() - myPos).normalized; // 1. 마우스 방향을 먼저 계산

        RaycastHit2D hit = controller.LightAimingLine.CalculateLightPath(myPos, _direction); // 2. 반사까지 포함한 경로 계산 (마지막으로 닿은 hit 반환)
        controller.LightAimingLine.DrawAimingLine(); // 3. 계산된 점들로 랜더링
        _canLightMove = controller.LightAimingLine.HandleHitLayer(hit); // 4. 마지막으로 닿은 게 뭔지 판단
        controller.LightAimingLine.SettingLightSensor();

        if (_canLightMove && GetMouseInput())
        {
            controller.LightAimingLine.SaveLightPath(hit, controller.LightPath); // 5. 이동 경로를 컨트롤러에 저장하고
            controller.ChangeMovementState(controller.LightMoveState); // 빛 이동 상태로 전환
        }
    }
}