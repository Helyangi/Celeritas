using System;
using System.Collections.Generic;
using UnityEngine;

public class LightAimingLine : MonoBehaviour
{
    [SerializeField] private Transform _lightPos;

    [SerializeField] private LayerMask ContactLayer; // 박스캐스트가 닿는 레이어 (거울, 빛 구체, 벽 등) / Glass는 체크해도 코드에서 빼고, 중심선 레이로 따로 판정한다
    [SerializeField] private int _maxReflection = 10; // 최대 반사 횟수 (유리는 들어갔다 나오는 것까지 1회로 센다)
    [SerializeField] private float _maxDistance = 40; // 조준선 최대 길이 (반사된 구간까지 합친 전체 길이)
    [SerializeField] private float _addDistance = 20; // 조준선 최대 길이 (반사된 구간까지 합친 전체 길이)
    [SerializeField] private float _width = 0.2f;
    [SerializeField] private Color _heatingColor;
    [SerializeField] private Color _coolingColor;
    private readonly List<Vector3> _points = new List<Vector3>(); // 조준선을 그릴 점들 (내 위치 -> 거울 꺾임 지점들 -> 끝점)
    private readonly List<Vector2> _waypoints = new List<Vector2>(); // 실제 이동에 쓸 거울 꺾임 지점들 (몸이 안 끼도록 거울에서 띄운 위치)
    private const float REFLECT_OFFSET = .01f; // 반사 후 다시 쏠 때 거울 표면에 또 맞지 않도록 띄우는 거리
    [HideInInspector] public List<Vector2> LightPath = new List<Vector2>(); // 빛 이동 시 순서대로 거쳐갈 경로 (Aiming 상태가 채우고 LightMove 상태가 읽음)
    public LightOrb TargetOrb { get; private set; } // 지금 이동 중인 목적지 빛 오브 (도착했을 때 알려주기 위해 저장)

    [Header("Glass (굴절)")]
    [SerializeField] private float _glassIndex = 1.5f;       // 유리 굴절률 (공기 = 1). 클수록 많이 꺾인다
    [SerializeField] private int _maxInternalBounce = 8;     // 유리 안에서 전반사로 튕길 수 있는 최대 횟수 (무한 루프 방지)
    private const string GLASS_LAYER_NAME = "Glass";
    private int _glassLayer = -1;                            // Glass 레이어 번호 (프로젝트에 없으면 -1이고 굴절은 꺼진다)
    private int _boxMask;                                    // 박스캐스트 마스크 (ContactLayer에서 Glass를 뺀 것)
    private int _glassMask;                                  // 중심선 레이 마스크 (Glass만)

    [Header("Glow (Bloom용 HDR 밝기)")]
    [SerializeField] private float _coolingIntensity = 2f;   // 빛 구체에 안 닿았을 때 밝기 (은은하게)
    [SerializeField] private float _heatingIntensity = 4f;  // 빛 구체에 닿았을 때 밝기 (강하게)

    [Header("Dust (조준선을 따라 떨어지는 모래 가루)")]
    [SerializeField] private ParticleSystem _dust;          // 자식 오브젝트의 파티클 (Emission은 꺼두고 코드로만 생성)
    [SerializeField] private float _dustPerUnitPerSecond = 3f; // 선 길이 1당 초당 생성 수 (선이 길어져도 밀도가 같게)
    [SerializeField] private float _dustSpread = 0.12f;     // 선에서 퍼지는 폭 (랜덤 오프셋 반경)
    [SerializeField] private float _dustSideSpeed = 0.25f;  // 생성 직후 좌우로 흩어지는 속도 (떨어지는 건 Gravity Modifier가 담당)
    [SerializeField] private int _dustMaxPerFrame = 40;     // 한 프레임에 생성하는 최대 수 (프레임 튈 때 폭발 방지)
    private float _dustCarry; // 소수점으로 남은 생성량을 다음 프레임으로 넘겨주는 누적값

    [Header("Dust (빛 이동 중 지나온 길에 떨어지는 가루)")]
    [SerializeField] private float _moveDustPerUnit = 4f;    // 이동 거리 1당 생성 수 (속도가 달라도 간격이 같게)
    [SerializeField] private float _moveDustSpread = 0.15f;  // 이동 경로에서 퍼지는 폭
    private float _moveDustCarry; // 소수점으로 남은 생성량을 다음 호출로 넘겨주는 누적값

    // 리스트
    private HashSet<LightSensor> _previousHit = new HashSet<LightSensor>();
    private HashSet<LightSensor> _currentHit = new HashSet<LightSensor>();

    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private LineRenderer _lineRenderer;
    private Material _material; // LineRenderer 머티리얼 인스턴스 (밝기를 코드에서 바꾸기 위해 캐싱)

    public void Awake()
    {
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>(); // 예외 처리

        _lineRenderer.startWidth = _width;
        _lineRenderer.endWidth = _width;
        _lineRenderer.numCornerVertices = 5; // 거울에서 꺾이는 부분을 둥글게 이어줌

        _material = _lineRenderer.material; // 인스턴스 복제본 (원본 .mat 파일은 건드리지 않음)
        SetIntensity(_coolingIntensity);

        _glassLayer = LayerMask.NameToLayer(GLASS_LAYER_NAME);
        _boxMask = ContactLayer.value;
        if (_glassLayer >= 0)
        {
            _glassMask = 1 << _glassLayer;
            _boxMask &= ~_glassMask; // 유리는 박스캐스트가 아니라 중심선 레이로만 판정한다
        }
        else Debug.LogWarning($"'{GLASS_LAYER_NAME}' 레이어가 없어 유리 굴절이 꺼져 있습니다.");
    }
    public void Start()
    {
        
    }
    private void OnDestroy()
    {
        if (_material != null) Destroy(_material); // 복제된 머티리얼 정리
    }
    public void SetAiming(bool value)
    {
        if (!value) _lineRenderer.positionCount = 0;
        _lineRenderer.enabled = value;
        enabled = value;
    }
    public RaycastHit2D CalculateLightPath(Vector2 direction) // 조준선 경로를 계산해서 _points / _waypoints를 채우는 함수
    {
        Vector2 start = _lightPos.position;

        _points.Clear();
        _waypoints.Clear();
        _points.Add(start); // 첫 점은 내 위치

        Vector2 origin = start;
        Vector2 dir = direction; // 이번 레이의 방향
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float remaining = _maxDistance; // 남은 조준선 길이
        RaycastHit2D hit = default;

        for (int i = 0; i <= _maxReflection; i++)
        {
            hit = CastLight(origin, dir, angle, remaining);

            if (!hit) // 아무것도 안 닿음 → 남은 거리만큼 직진한 곳이 끝점
            {
                _points.Add(origin + dir * remaining);
                break;
            }

            bool isGlass = _glassLayer >= 0 && hit.collider.gameObject.layer == _glassLayer;
            _points.Add(isGlass ? hit.point : hit.centroid); // 닿은 지점 = 선이 꺾이거나 끝나는 점 (유리는 레이 hit이라 centroid가 아니라 point)

            AddHit(hit.collider); // 유리도 LightSensor가 있으면 여기서 발광 대상이 된다

            bool isFrontSide = Vector2.Dot(hit.normal, -hit.transform.right) > 0.99f; // 거울 앞면 맞았는지 검사
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Mirror") && isFrontSide) // 거울이 아니면(빛 구체, 벽 등) 여기서 끝
            {
                // 거울이면: 이동용 꺾임 지점 저장 → 남은 거리 갱신 → 반사 방향 계산 → 다음 레이 준비
                _waypoints.Add(hit.centroid);

                remaining += _addDistance;
                remaining -= hit.distance;
                if (remaining <= 0f) break;

                dir = Vector2.Reflect(dir, hit.normal); // 반사 공식: r = d - 2(d·n)n
                origin = hit.centroid + hit.normal * REFLECT_OFFSET;
                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }
            else if (isGlass)
            {
                // 유리면: 굴절해서 들어가고 → 안에서 출구를 찾고 → 다시 굴절해서 나온다 (전반사면 안에서 튕긴다)
                if (!PassThroughGlass(hit, ref dir, ref remaining, out origin)) break;

                angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            }
            else
            {
                break;
            }
        }

        return hit;
    }
    private RaycastHit2D CastLight(Vector2 origin, Vector2 dir, float angle, float distance) // 박스(벽, 거울, 빛 구체)와 중심선 레이(유리) 중 더 가까운 hit을 돌려준다
    {
        RaycastHit2D box = Physics2D.BoxCast(origin, new Vector2(0.001f, _width), angle, dir, distance, _boxMask);
        RaycastHit2D ray = Physics2D.Raycast(origin, dir, distance, _glassMask); // 유리는 폭이 없는 중심선으로만 판정 → 항상 표면 위의 정확한 점이 나온다

        if (ray && (!box || ray.distance < box.distance)) return ray;
        return box;
    }
    // 유리를 통과하는 함수. 성공하면 true (dir/remaining/nextOrigin을 갱신), 선이 유리 안에서 끝나거나 막히면 false
    // hit은 중심선 레이 hit이므로 hit.point가 정확한 진입점이고, hit.normal은 들어오는 빛을 마주보는 방향이다
    private bool PassThroughGlass(RaycastHit2D hit, ref Vector2 dir, ref float remaining, out Vector2 nextOrigin)
    {
        nextOrigin = hit.point;
        Collider2D glass = hit.collider;

        _waypoints.Add(hit.point); // 이동용 꺾임 지점: 진입점

        remaining += _addDistance;
        remaining -= hit.distance;
        if (remaining <= 0f) return false;

        // 1. 공기 → 유리 굴절 (공기→유리는 전반사가 없다)
        if (!Refract(dir, hit.normal, 1f, _glassIndex, out Vector2 insideDir)) return false;
        dir = insideDir;
        Vector2 pos = hit.point + dir * REFLECT_OFFSET; // 유리 안쪽으로 살짝 들어간 위치

        // 2. 유리 안: 출구를 찾는다 (전반사로 튕기면 반복)
        for (int bounce = 0; bounce <= _maxInternalBounce; bounce++)
        {
            Vector2 end = pos + dir * remaining; // 남은 길이만큼 갔을 때의 끝점
            if (glass.OverlapPoint(end)) // 끝점이 아직 유리 안이면 선이 유리 안에서 끝난다
            {
                _points.Add(end);
                remaining = 0f;
                return false;
            }

            // 끝점(유리 밖)에서 진행 반대 방향으로 쏘면 가장 먼저 닿는 유리 면이 출구다.
            // 안에서 앞으로 쏘면 queriesStartInColliders 때문에 자기 자신에 거리 0으로 맞기 때문에 이렇게 거꾸로 쏜다.
            RaycastHit2D exit = default;
            RaycastHit2D[] backHits = Physics2D.RaycastAll(end, -dir, remaining, _glassMask);
            foreach (RaycastHit2D h in backHits)
            {
                if (h.collider != glass) continue;
                exit = h;
                break;
            }
            if (!exit) // 출구를 못 찾은 예외 상황 → 여기서 선을 끝낸다
            {
                _points.Add(pos);
                remaining = 0f;
                return false;
            }

            Vector2 exitPoint = exit.point;
            remaining -= Vector2.Distance(pos, exitPoint);
            _points.Add(exitPoint);
            _waypoints.Add(exitPoint); // 이동용 꺾임 지점: 출구(또는 전반사 지점)

            Vector2 inward = -exit.normal; // exit.normal은 유리 바깥쪽을 향한다 → 진행 방향과 반대인 안쪽 법선

            // 3. 유리 → 공기 굴절. 성공하면 밖으로 나가고, 실패하면 전반사
            if (Refract(dir, inward, _glassIndex, 1f, out Vector2 outDir))
            {
                dir = outDir;
                nextOrigin = exitPoint + exit.normal * REFLECT_OFFSET; // 표면에서 살짝 띄워서 다시 쏜다
                return remaining > 0f;
            }

            dir = Vector2.Reflect(dir, exit.normal); // 전반사: 유리 안에서 반사
            pos = exitPoint + inward * REFLECT_OFFSET;
            if (remaining <= 0f) return false;
        }

        return false; // 전반사가 너무 많이 반복되면 포기 (무한 루프 방지)
    }
    private static bool Refract(Vector2 dir, Vector2 normal, float n1, float n2, out Vector2 refracted) // 스넬의 법칙으로 굴절 방향을 구한다. 전반사면 false
    {
        // normal은 입사 방향과 반대쪽(들어오는 빛을 마주보는 쪽)을 향해야 한다
        float eta = n1 / n2;
        float cosI = -Vector2.Dot(normal, dir);
        float k = 1f - eta * eta * (1f - cosI * cosI); // 음수면 굴절각이 90°를 넘는다 = 전반사

        if (k < 0f)
        {
            refracted = Vector2.zero;
            return false;
        }

        refracted = (eta * dir + (eta * cosI - Mathf.Sqrt(k)) * normal).normalized;
        return true;
    }
    public void SaveLightPath(RaycastHit2D finalHit, List<Vector2> path) // 빛 이동 상태가 따라갈 경로를 컨트롤러에 저장하는 함수
    {
        path.Clear();
        path.AddRange(_waypoints); // 거울 꺾임 지점들 (순서대로)
        path.Add(finalHit.transform.position); // 마지막은 빛 구체 위치
        TargetOrb = finalHit.collider != null && finalHit.collider.TryGetComponent(out LightOrb orb) ? orb : null;
    }
    public bool HandleHitLayer(RaycastHit2D hit) // 닿은 레이어가 뭔지 판단하는 함수
    {
        if (hit.collider == null)
        {
            CoolingColor();
            return false;
        }

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("LightOrb"))
        {
            HeatingColor();
            return true;
        }
        else
        {
            CoolingColor();
            return false;
        }
    }
    public void DrawAimingLine() // 게임에서 조준선 그려주는 함수 (점 리스트를 순서대로 이어서 그림)
    {
        _lineRenderer.positionCount = _points.Count; // 이번 프레임에 쓴 점 개수만큼만 그린다
        for (int i = 0; i < _points.Count; i++)
        {
            _lineRenderer.SetPosition(i, _points[i]);
        }

        EmitDust(); // 같은 _points를 그대로 써서 선을 따라 모래 가루를 생성 (경로를 다시 계산하지 않음)
    }
    private void EmitDust() // 조준선(_points) 위의 랜덤한 위치에서 파티클을 생성하는 함수
    {
        if (_dust == null || _points.Count < 2) return;

        // 1. 선 전체 길이
        float total = 0f;
        for (int i = 1; i < _points.Count; i++)
        {
            total += Vector3.Distance(_points[i - 1], _points[i]);
        }
        if (total < 0.01f) return;

        // 2. 이번 프레임에 만들 개수 (소수점은 다음 프레임으로 넘김)
        _dustCarry += _dustPerUnitPerSecond * total * Time.deltaTime;
        int count = Mathf.Min((int)_dustCarry, _dustMaxPerFrame);
        _dustCarry -= (int)_dustCarry;

        // 3. 선 위의 랜덤 지점에 생성
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        for (int n = 0; n < count; n++)
        {
            emitParams.position = PointOnLine(UnityEngine.Random.value * total) + (Vector3)(UnityEngine.Random.insideUnitCircle * _dustSpread);
            emitParams.velocity = new Vector3(UnityEngine.Random.Range(-_dustSideSpeed, _dustSideSpeed), 0f, 0f); // 아래로 떨어지는 건 Gravity Modifier
            _dust.Emit(emitParams, 1);
        }
    }
    // 빛 이동 중 from → to로 움직인 구간 위에 가루를 생성한다. (조준 중 가루와 같은 파티클·같은 낙하 설정을 쓴다)
    public void EmitDustAlong(Vector2 from, Vector2 to)
    {
        if (_dust == null) return;

        float distance = Vector2.Distance(from, to);
        if (distance < 0.0001f) return;

        _moveDustCarry += _moveDustPerUnit * distance;
        int count = Mathf.Min((int)_moveDustCarry, _dustMaxPerFrame);
        _moveDustCarry -= (int)_moveDustCarry;

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        for (int n = 0; n < count; n++)
        {
            Vector2 pos = Vector2.Lerp(from, to, UnityEngine.Random.value);
            emitParams.position = pos + UnityEngine.Random.insideUnitCircle * _moveDustSpread;
            emitParams.velocity = new Vector3(UnityEngine.Random.Range(-_dustSideSpeed, _dustSideSpeed), 0f, 0f);
            _dust.Emit(emitParams, 1);
        }
    }
    private Vector3 PointOnLine(float distance) // 선 시작점에서 distance만큼 간 지점의 위치 (구간마다 길이를 빼가며 찾음)
    {
        for (int i = 1; i < _points.Count; i++)
        {
            float segment = Vector3.Distance(_points[i - 1], _points[i]);
            if (distance <= segment && segment > 0f)
            {
                return Vector3.Lerp(_points[i - 1], _points[i], distance / segment);
            }
            distance -= segment;
        }
        return _points[_points.Count - 1];
    }
    private void HeatingColor()
    {
        _lineRenderer.startColor = _heatingColor;
        _lineRenderer.endColor = _heatingColor;
        SetIntensity(_heatingIntensity);
    }
    
    private void CoolingColor()
    {
        _lineRenderer.startColor = _coolingColor;
        _lineRenderer.endColor = _coolingColor;
        SetIntensity(_coolingIntensity);
    }

    private void SetIntensity(float value) // 머티리얼의 _Intensity 값을 바꿔서 Bloom이 반응하는 정도를 조절
    {
        if (_material != null) _material.SetFloat(IntensityId, value);
    }
    private void AddHit(Collider2D collider)
    {
        if (collider.TryGetComponent(out LightSensor sensor))
        {
            _currentHit.Add(sensor);
        }
    }
    public void ClearLightSensors() // 조준이 끝났을 때 닿아 있던 센서의 발광을 모두 끈다
    {
        foreach (LightSensor sensor in _previousHit)
        {
            sensor.DisableLight();
        }

        _previousHit.Clear();
        _currentHit.Clear();
    }
    public void SettingLightSensor()
    {
        foreach (LightSensor sensor in _currentHit)
        {
            sensor.EnableLight();
        }

        _previousHit.ExceptWith(_currentHit);

        foreach (LightSensor sensor in _previousHit)
        {
            sensor.DisableLight();
        }

        (_previousHit, _currentHit) = (_currentHit, _previousHit);
        _currentHit.Clear();
    }
}
