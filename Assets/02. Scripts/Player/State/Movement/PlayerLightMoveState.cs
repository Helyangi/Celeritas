using System.Collections.Generic;
using UnityEngine;

public class PlayerLightMoveState : PlayerBaseMovementState
{
    private int _pathIndex; // 지금 향하고 있는 경로 번호 (controller.LightPath의 인덱스)
    private float _speed;   // 이번 이동에 쓸 속도. 경로 길이를 이동 시간으로 나눠서 매번 새로 구한다
    private float _intensity = 2;
    private const float ReflectPitchStep = 2f; // 반사 1회마다 올릴 반음 수
    private float _initialGravityScale;
    public PlayerLightMoveState(PlayerController controller) : base(controller) {}
    private Vector2 _boxVector = new Vector2(0.5f, 0.5f);
    
    public override void EnterState()
    {

        controller.Light.intensity = _intensity;
        _initialGravityScale = controller.RB.gravityScale;
        controller.RB.gravityScale = 0;
        controller.Anim.Play("LightMove");
        controller.ChangeActionState(controller.NoneState);
        SoundManager.Instance.PlaySFX(SFXType.PlayerLightShot, controller.transform.position);
        Hitstop.Play(controller.Stats.LightShotHitstop); // 발사 직전에 한 박자 멈춰 쏘는 맛을 준다
        _pathIndex = 0; // 항상 경로의 첫 지점부터 시작
        
        // 콜라이더 바꾸기
        controller.BoxCol.isTrigger = true;
        controller.BoxCol.size = _boxVector;

        _speed = CalculateSpeed();

        Vector2 from = controller.RB.position;
        Vector2 shotTarget = controller.LightAimingLine.LightPath[_pathIndex];
        LightRotate(from, shotTarget);
        controller.RaiseLightShot(from, (shotTarget - from).normalized);
    }

    // 경로가 길든 짧든 같은 시간이 걸리도록 속도를 구한다.
    // (거울이 몇 번 꺾이는지와 상관없이 "빛이 번쩍 지나갔다"가 일정하게 읽히게 하기 위해서다)
    private float CalculateSpeed()
    {
        float duration = controller.Stats.LightMoveDuration;
        if (duration <= 0f) return controller.Stats.LightMoveSpeed; // 예전처럼 고정 속도로 움직인다

        List<Vector2> path = controller.LightAimingLine.LightPath;
        float total = 0f;
        Vector2 from = controller.RB.position;
        for (int i = 0; i < path.Count; i++)
        {
            total += Vector2.Distance(from, path[i]);
            from = path[i];
        }

        if (total <= 0f) return controller.Stats.MaxLightMoveSpeed;

        return Mathf.Clamp(total / duration, controller.Stats.MinLightMoveSpeed, controller.Stats.MaxLightMoveSpeed);
    }
    
    public override void FixedUpdateState()
    {
        LightMove();
    }
    
    private void LightMove()
    {
        List<Vector2> path = controller.LightAimingLine.LightPath;

        float budget = _speed * Time.fixedDeltaTime; // 이번 물리 스텝에 움직일 수 있는 거리
        Vector2 position = controller.RB.position;

        // 한 스텝 안에서 거울을 여러 번 지나갈 수 있다. 남은 거리를 다 쓸 때까지 경로를 따라간다
        // (지점마다 한 스텝씩 끊어 쓰면 거울이 많을수록 느려져서 이동 시간이 들쑥날쑥해진다)
        while (budget > 0f)
        {
            if (_pathIndex >= path.Count)
            {
                controller.RB.MovePosition(position);
                FinishMove();
                return;
            }

            Vector2 target = path[_pathIndex];
            float distance = Vector2.Distance(position, target);

            // 이번 스텝 안에 다음 지점까지 못 간다: 갈 수 있는 만큼만 가고 끝낸다
            if (distance > budget)
            {
                Vector2 next = Vector2.MoveTowards(position, target, budget);
                controller.LightAimingLine.EmitDustAlong(position, next); // 지나온 길에 빛 가루를 남긴다
                position = next;
                break;
            }

            // 다음 지점에 도달했다: 쓴 만큼 깎고 경로를 넘어간다
            controller.LightAimingLine.EmitDustAlong(position, target);
            position = target;
            budget -= distance;
            _pathIndex++;

            if (_pathIndex >= path.Count)
            {
                controller.RB.MovePosition(position);
                FinishMove();
                return;
            }

            // 거울에서 꺾이는 순간. 반사할 때마다 음을 올려 경로가 몇 번 꺾였는지 귀로 알 수 있게 한다.
            SoundManager.Instance.PlaySFX(SFXType.MirrorReflect, position, Mathf.Pow(2f, (_pathIndex - 1) * ReflectPitchStep / 12f));
            Hitstop.Play(controller.Stats.MirrorReflectHitstop); // 꺾이는 순간을 하나의 사건으로 느끼게 한다
            LightRotate(position, path[_pathIndex]);
            controller.RaiseLightReflected(position, (path[_pathIndex] - position).normalized);
        }

        controller.RB.MovePosition(position);
    }
    // 마지막 지점(빛 오브)에 도달했을 때: 오브에 도착을 알리고(파티클 + 쿨타임) 낙하 상태로 넘어간다
    private void FinishMove()
    {
        LightOrb orb = controller.LightAimingLine.TargetOrb;
        if (orb != null) orb.OnArrived();
        SoundManager.Instance.PlaySFX(SFXType.PlayerLightArrival, controller.transform.position); // 실제로 도착했을 때만 울린다
        Hitstop.Play(controller.Stats.LightArriveHitstop); // 도착을 가장 길게 멈춰 마무리를 찍어준다
        controller.ChangeMovementState(controller.FallState);
    }
    private void LightRotate(Vector2 from, Vector2 target)
    {
        Vector2 dir = target - from;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        controller.Renderer.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    public override void ExitState()
    {

        controller.Light.intensity = 1;
        controller.RB.gravityScale = _initialGravityScale;
        controller.Renderer.transform.rotation = Quaternion.identity;
        controller.LightAimingLine.ClearLightSensors(); // 도착(또는 중단)하면 빛 구체의 발광을 끈다
        controller.RB.linearVelocityY = 0;

        // 박스 콜라이더
        controller.BoxCol.isTrigger = false;
        controller.BoxCol.size = controller.InitialVector;

        controller.CanJump = true;
    }
}
