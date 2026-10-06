using System.Collections.Generic;
using UnityEngine;

public class PlayerAimingState : PlayerBaseActionState
{
    public PlayerAimingState(PlayerController controller) : base(controller) {}
    
    private Vector2 _direction;
    private bool _canLightMove;
    private readonly List<Vector3> _points = new List<Vector3>(); // 조준선을 그릴 점들 (내 위치 → 거울 꺾임 지점들 → 끝점)
    private readonly List<Vector2> _waypoints = new List<Vector2>(); // 실제 이동에 쓸 거울 꺾임 지점들 (몸이 안 끼도록 거울에서 띄운 위치)
    private const float ReflectOffset = 0.01f; // 반사 후 다시 쏠 때 거울 표면에 또 맞지 않도록 띄우는 거리

    public override void UpdateState()
    {
        Vector2 myPos = controller.RB.position;
        _direction = (GetMousePos() - myPos).normalized; // 1. 마우스 방향을 먼저 계산

        RaycastHit2D hit = CalculateLightPath(myPos, _direction); // 2. 반사까지 포함한 경로 계산 (마지막으로 닿은 hit 반환)
        controller.LightAimingLine.DrawAimingLine(_points); // 3. 계산된 점들로 랜더링
        HandleHitLayer(hit); // 4. 마지막으로 닿은 게 뭔지 판단

        if (_canLightMove && GetMouseInput())
        {
            SaveLightPath(hit); // 5. 이동 경로를 컨트롤러에 저장하고
            controller.ChangeMovementState(controller.LightMoveState); // 빛 이동 상태로 전환
        }
    }
    private RaycastHit2D CalculateLightPath(Vector2 start, Vector2 direction) // 조준선 경로를 계산해서 _points / _waypoints를 채우는 함수
    {
        LightAimingLine line = controller.LightAimingLine;

        _points.Clear();
        _waypoints.Clear();
        _points.Add(start); // 첫 점은 내 위치

        Vector2 origin = start; // 이번 레이를 쏘는 시작점
        Vector2 dir = direction; // 이번 레이의 방향
        float remaining = line.MaxDistance; // 남은 조준선 길이
        RaycastHit2D hit = default;

        for (int i = 0; i <= line.MaxReflection; i++)
        {
            hit = Physics2D.Raycast(origin, dir, remaining, line.ContactLayer);

            if (!hit) // 아무것도 안 닿음 → 남은 거리만큼 직진한 곳이 끝점
            {
                _points.Add(origin + dir * remaining);
                break;
            }

            _points.Add(hit.point); // 닿은 지점 = 선이 꺾이거나 끝나는 점

            if (!line.IsMirror(hit.collider)) break; // 거울이 아니면(빛 구체, 벽 등) 여기서 끝

            // 거울이면: 이동용 꺾임 지점 저장 → 남은 거리 갱신 → 반사 방향 계산 → 다음 레이 준비
            _waypoints.Add(hit.point + hit.normal * controller.Stats.LightMoveCornerOffset);

            remaining -= hit.distance;
            if (remaining <= 0f) break;

            dir = Vector2.Reflect(dir, hit.normal); // 반사 공식: r = d - 2(d·n)n
            origin = hit.point + hit.normal * ReflectOffset;
        }

        return hit;
    }
    private void SaveLightPath(RaycastHit2D finalHit) // 빛 이동 상태가 따라갈 경로를 컨트롤러에 저장하는 함수
    {
        List<Vector2> path = controller.LightPath;
        path.Clear();
        path.AddRange(_waypoints); // 거울 꺾임 지점들 (순서대로)
        path.Add(finalHit.transform.position); // 마지막은 빛 구체 위치
    }
    
    private void HandleHitLayer(RaycastHit2D hit) // 닿은 레이어가 뭔지 판단하는 함수
    {
        if (hit.collider == null)
        {
            _canLightMove = false;
            controller.LightAimingLine.CoolingColor();
            return;
        }

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("LightOrb"))
        {
            _canLightMove = true;
            controller.LightAimingLine.HeatingColor();
        }
        else
        {
            _canLightMove = false;
            controller.LightAimingLine.CoolingColor();
        }
    }
}
