using System.Collections.Generic;
using UnityEngine;

public class PlayerLightMoveState : PlayerBaseMovementState
{
    private int _pathIndex; // 지금 향하고 있는 경로 번호 (controller.LightPath의 인덱스)
    private const float ArriveDistance = 0.1f; // 목표 지점에 이 거리 안으로 들어오면 도착한 것으로 처리

    public PlayerLightMoveState(PlayerController controller) : base(controller) {}
    
    public override void EnterState()
    {
        controller.CanJump = true;
        _pathIndex = 0; // 항상 경로의 첫 지점부터 시작
    }
    
    public override void FixedUpdateState()
    {
        LightMove();
    }
    
    private void LightMove()
    {
        List<Vector2> path = controller.LightPath;

        // 경로가 비었거나 이미 다 지나왔으면 낙하 상태로 전환
        if (_pathIndex >= path.Count)
        {
            controller.ChangeMovementState(controller.FallState);
            return;
        }

        // 현재 목표 지점을 향해 한 걸음 이동
        Vector2 target = path[_pathIndex];
        Vector2 newPos = Vector2.MoveTowards
        (
            controller.RB.position,
            target,
            controller.Stats.LightMoveSpeed * Time.fixedDeltaTime
        );
        controller.RB.MovePosition(newPos);

        // 목표 지점에 도착했으면 다음 지점으로, 마지막 지점이었다면 낙하 상태로 전환
        if (Vector2.Distance(newPos, target) <= ArriveDistance)
        {
            _pathIndex++;
            if (_pathIndex >= path.Count)
            {
                controller.ChangeMovementState(controller.FallState);
            }
        }
    }
    
    public override void ExitState()
    {
        controller.RB.linearVelocityY = 0;
    }
}
