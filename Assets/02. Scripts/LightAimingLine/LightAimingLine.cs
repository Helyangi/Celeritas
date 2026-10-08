using System.Collections.Generic;
using UnityEngine;

public class LightAimingLine : MonoBehaviour
{
    [SerializeField] private LayerMask ContactLayer; // 레이가 닿는 레이어 (거울, 빛 구체, 벽 등을 모두 포함해야 함)
    [SerializeField] private int _maxReflection = 10; // 최대 반사 횟수
    [SerializeField] private float _maxDistance = 40; // 조준선 최대 길이 (반사된 구간까지 합친 전체 길이)
    [SerializeField] private Color _heatingColor;
    [SerializeField] private Color _coolingColor;
    private float _lightMoveCornerOffset = 0.6f; // 빛 이동 시 거울 꺾임 지점을 거울 표면에서 띄우는 거리 (플레이어 몸 크기보다 크게)
    private readonly List<Vector3> _points = new List<Vector3>(); // 조준선을 그릴 점들 (내 위치 -> 거울 꺾임 지점들 -> 끝점)
    private readonly List<Vector2> _waypoints = new List<Vector2>(); // 실제 이동에 쓸 거울 꺾임 지점들 (몸이 안 끼도록 거울에서 띄운 위치)
    private const float ReflectOffset = 0.01f; // 반사 후 다시 쏠 때 거울 표면에 또 맞지 않도록 띄우는 거리

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

    // 리스트
    private HashSet<LightSensor> _previousHit = new HashSet<LightSensor>();
    private HashSet<LightSensor> _currentHit = new HashSet<LightSensor>();

    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private LineRenderer _lineRenderer;
    private Material _material; // LineRenderer 머티리얼 인스턴스 (밝기를 코드에서 바꾸기 위해 캐싱)

    public void Awake()
    {
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>(); // 예외 처리

        _lineRenderer.startWidth = 0.2f;
        _lineRenderer.endWidth = 0.2f;
        _lineRenderer.numCornerVertices = 5; // 거울에서 꺾이는 부분을 둥글게 이어줌

        _material = _lineRenderer.material; // 인스턴스 복제본 (원본 .mat 파일은 건드리지 않음)
        SetIntensity(_coolingIntensity);
    }
    public void Start()
    {
        
    }
    private void OnDestroy()
    {
        if (_material != null) Destroy(_material); // 복제된 머티리얼 정리
    }
    public RaycastHit2D CalculateLightPath(Vector2 start, Vector2 direction) // 조준선 경로를 계산해서 _points / _waypoints를 채우는 함수
    {
        _points.Clear();
        _waypoints.Clear();
        _points.Add(start); // 첫 점은 내 위치

        Vector2 origin = start; // 이번 레이를 쏘는 시작점
        Vector2 dir = direction; // 이번 레이의 방향
        float remaining = _maxDistance; // 남은 조준선 길이
        RaycastHit2D hit = default;

        for (int i = 0; i <= _maxReflection; i++)
        {
            hit = Physics2D.Raycast(origin, dir, remaining, ContactLayer);

            if (!hit) // 아무것도 안 닿음 → 남은 거리만큼 직진한 곳이 끝점
            {
                _points.Add(origin + dir * remaining);
                break;
            }

            _points.Add(hit.point); // 닿은 지점 = 선이 꺾이거나 끝나는 점

            AddHit(hit.collider);

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Mirror")) // 거울이 아니면(빛 구체, 벽 등) 여기서 끝
            {
                // 거울이면: 이동용 꺾임 지점 저장 → 남은 거리 갱신 → 반사 방향 계산 → 다음 레이 준비
                _waypoints.Add(hit.point + hit.normal * _lightMoveCornerOffset);

                remaining -= hit.distance;
                if (remaining <= 0f) break;

                dir = Vector2.Reflect(dir, hit.normal); // 반사 공식: r = d - 2(d·n)n
                origin = hit.point + hit.normal * ReflectOffset;
            }
            else
            {
                break;
            }
        }

        return hit;
    }
    public void SaveLightPath(RaycastHit2D finalHit, List<Vector2> path) // 빛 이동 상태가 따라갈 경로를 컨트롤러에 저장하는 함수
    {
        path.Clear();
        path.AddRange(_waypoints); // 거울 꺾임 지점들 (순서대로)
        path.Add(finalHit.transform.position); // 마지막은 빛 구체 위치
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
        var emitParams = new ParticleSystem.EmitParams();
        for (int n = 0; n < count; n++)
        {
            emitParams.position = PointOnLine(Random.value * total) + (Vector3)(Random.insideUnitCircle * _dustSpread);
            emitParams.velocity = new Vector3(Random.Range(-_dustSideSpeed, _dustSideSpeed), 0f, 0f); // 아래로 떨어지는 건 Gravity Modifier
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
