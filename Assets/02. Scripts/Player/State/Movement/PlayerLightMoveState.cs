using System.Collections.Generic;
using UnityEngine;

public class PlayerLightMoveState : PlayerBaseMovementState
{
    private int _pathIndex; // 지금 향하고 있는 경로 번호 (controller.LightPath의 인덱스)
    private float _intensity = 2;
    private const float ReflectPitchStep = 2f; // 반사 1회마다 올릴 반음 수
    private const float ArriveDistance = 0.01f; // 목표 지점에 이 거리 안으로 들어오면 도착한 것으로 처리
    private float _initialGravityScale;
    public PlayerLightMoveState(PlayerController controller) : base(controller) {}
    private Vector2 _boxVector = new Vector2(0.5f, 0.5f);
    
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
            FinishMove();
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
                FinishMove();
                return;
            }

            // 거울에서 꺾이는 순간. 반사할 때마다 음을 올려 경로가 몇 번 꺾였는지 귀로 알 수 있게 한다.
            SoundManager.Instance.PlaySFX(SFXType.MirrorReflect, newPos, Mathf.Pow(2f, (_pathIndex - 1) * ReflectPitchStep / 12f));
            LightRotate(path[_pathIndex]);
        }
    }
    // 마지막 지점(빛 오브)에 도달했을 때: 오브에 도착을 알리고(파티클 + 쿨타임) 낙하 상태로 넘어간다
    private void FinishMove()
    {
        LightOrb orb = controller.LightAimingLine.TargetOrb;
        if (orb != null) orb.OnArrived();
        controller.ChangeMovementState(controller.FallState);
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

        controller.CanJump = true;
    }
}
