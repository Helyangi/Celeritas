using System.Collections.Generic;
using UnityEngine;

public class PlayerLightMoveState : PlayerBaseMovementState
{
    private int _pathIndex; // 지금 향하고 있는 경로 번호 (controller.LightPath의 인덱스)
    private float _intensity = 2;
    private const float ArriveDistance = 0.01f; // 목표 지점에 이 거리 안으로 들어오면 도착한 것으로 처리
    private float _initialGravityScale;
    public PlayerLightMoveState(PlayerController controller) : base(controller) {}
    private Vector2 _boxVector = new Vector2(0.5f, 0.5f);
    private Vector2 _boxOffset = new Vector2(0.6f, 0);
    
    public override void EnterState()
    {

        // Trail Renderer 켜기
        controller.Trail.emitting = true;
        controller.Trail.material.SetFloat(Shader.PropertyToID("_Intensity"), _intensity);

        controller.Light.intensity = _intensity;
        _initialGravityScale = controller.RB.gravityScale;
        controller.RB.gravityScale = 0;
        controller.Anim.Play("LightMove");
        controller.ChangeActionState(controller.NoneState);
        SoundManager.Instance.PlaySFX(SFXType.PlayerLightShot, controller.transform.position);
        _pathIndex = 0; // 항상 경로의 첫 지점부터 시작
        
        // 콜라이더 바꾸기
        controller.BoxCol.isTrigger = true;
        controller.BoxCol.size = _boxVector;
        controller.BoxCol.offset = _boxOffset;

        LightRotate(controller.LightAimingLine.LightPath[_pathIndex]);
    }
    
    public override void FixedUpdateState()
    {
        LightMove();
    }
    
    private void LightMove()
    {
        List<Vector2> path = controller.LightAimingLine.LightPath;

        // 경로가 비었거나 이미 다 지나왔으면 낙하 상태로 전환
        if (_pathIndex >= path.Count)
        {
            controller.ChangeMovementState(controller.FallState);
            return;
        }

        // 현재 목표 지점을 향해 한 걸음 이동
        Vector2 target = path[_pathIndex];
        Vector2 previous = controller.RB.position;
        Vector2 newPos = Vector2.MoveTowards
        (
            controller.RB.position,
            target,
            controller.Stats.LightMoveSpeed * Time.fixedDeltaTime
        );
        controller.RB.MovePosition(newPos);
        controller.LightAimingLine.EmitDustAlong(previous, newPos); // 지나온 길에 빛 가루를 남긴다

        // 목표 지점에 도착했으면 다음 지점으로, 마지막 지점이었다면 낙하 상태로 전환
        if (Vector2.Distance(newPos, target) <= ArriveDistance)
        {
            _pathIndex++;
            if (_pathIndex >= path.Count)
            {
                controller.ChangeMovementState(controller.FallState);
                return;
            }

            LightRotate(path[_pathIndex]);
        }
    }
    private void LightRotate(Vector2 target)
    {
        Vector2 dir = target - (Vector2)controller.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        controller.Renderer.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    public override void ExitState()
    {
        // Trail Renderer 켜기
        controller.Trail.emitting = false;

        controller.Light.intensity = 1;
        controller.RB.gravityScale = _initialGravityScale;
        controller.Renderer.transform.rotation = Quaternion.identity;
        controller.LightAimingLine.ClearLightSensors(); // 도착(또는 중단)하면 빛 구체의 발광을 끈다
        SoundManager.Instance.PlaySFX(SFXType.PlayerLightArrival, controller.transform.position);
        controller.RB.linearVelocityY = 0;

        // 박스 콜라이더
        controller.BoxCol.isTrigger = false;
        controller.BoxCol.size = controller.InitialVector;
        controller.BoxCol.offset = Vector2.zero;

        controller.CanJump = true;
    }
}
