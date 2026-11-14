using UnityEngine;

/// <summary>
/// 배경에 고르게 떠다니는 빛 입자의 양을 인스펙터에서 바로 조절하기 위한 컴포넌트.
/// 파티클 시스템은 "초당 몇 개 생성"으로 설정하는데, 보고 싶은 건 "화면에 몇 개 떠 있는가"라
/// 수명이 바뀔 때마다 생성량을 다시 계산해야 한다. 그 계산을 여기서 대신한다.
/// 값을 바꾸면 에디터에서도 즉시 반영된다.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(ParticleSystem))]
public class BackgroundParticles : MonoBehaviour
{
    [Header("양")]
    [Tooltip("화면에 동시에 떠 있을 개수 (대략). 생성량은 수명에 맞춰 자동으로 계산된다")]
    [SerializeField, Min(0f)] private float _amount = 45f;

    [Header("입자")]
    [Tooltip("수명(초) 최소~최대. 길수록 오래 머문다")]
    [SerializeField] private Vector2 _lifetime = new Vector2(2.5f, 4f);
    [Tooltip("크기 최소~최대")]
    [SerializeField] private Vector2 _size = new Vector2(0.035f, 0.07f);
    [Tooltip("떠다니는 흔들림의 세기 (노이즈). 0이면 제자리에 멈춰 있다")]
    [SerializeField, Min(0f)] private float _drift = 0.15f;
    [Tooltip("흔들림이 변하는 속도. 클수록 부산하다")]
    [SerializeField, Min(0f)] private float _driftSpeed = 0.2f;

    [Header("범위")]
    [Tooltip("생성 영역을 카메라 화면에 맞춘다")]
    [SerializeField] private bool _fitToCamera = true;
    [Tooltip("화면보다 얼마나 더 넓게 뿌릴지 (1 = 화면과 같음)")]
    [SerializeField, Min(1f)] private float _margin = 1.1f;
    [Tooltip("카메라에 맞추지 않을 때 쓸 생성 영역 크기")]
    [SerializeField] private Vector2 _area = new Vector2(19f, 11f);

    private ParticleSystem _particles;

    private void Awake() => Apply();

    private void OnEnable() => Apply();

    private void OnValidate() => Apply();

    private void Apply()
    {
        if (_particles == null) _particles = GetComponent<ParticleSystem>();
        if (_particles == null) return;

        ParticleSystem.MainModule main = _particles.main;
        main.startLifetime = new ParticleSystem.MinMaxCurve(_lifetime.x, _lifetime.y);
        main.startSize = new ParticleSystem.MinMaxCurve(_size.x, _size.y);

        // 동시에 떠 있는 수 ≈ 초당 생성량 × 평균 수명 → 생성량 = 원하는 수 ÷ 평균 수명
        float averageLifetime = Mathf.Max(0.01f, (_lifetime.x + _lifetime.y) * 0.5f);
        float rate = _amount / averageLifetime;

        ParticleSystem.EmissionModule emission = _particles.emission;
        emission.rateOverTime = new ParticleSystem.MinMaxCurve(rate);

        main.maxParticles = Mathf.Max(16, Mathf.CeilToInt(_amount * 2f)); // 여유를 둔다

        ParticleSystem.NoiseModule noise = _particles.noise;
        noise.enabled = _drift > 0f;
        noise.strength = new ParticleSystem.MinMaxCurve(_drift);
        noise.scrollSpeed = new ParticleSystem.MinMaxCurve(_driftSpeed);

        ParticleSystem.ShapeModule shape = _particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.position = Vector3.zero;
        shape.scale = new Vector3(Area.x, Area.y, 0.01f);
    }

    private Vector2 Area
    {
        get
        {
            if (!_fitToCamera) return _area;

            Camera camera = Camera.main;
            if (camera == null || !camera.orthographic) return _area;

            float height = camera.orthographicSize * 2f;
            return new Vector2(height * camera.aspect, height) * _margin;
        }
    }
}
