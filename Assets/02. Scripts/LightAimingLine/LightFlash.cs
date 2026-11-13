using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 빛을 쏘는 순간 그 경로가 번쩍였다가 사그라드는 연출.
/// 조준선(LightAimingLine)과는 완전히 별개의 LineRenderer를 쓴다.
/// 발사 시점의 경로를 복사해 두기 때문에, 쏘자마자 다시 조준해도
/// 조준선이 가려지거나 플래시가 새 조준 경로로 바뀌는 일이 없다.
/// 좌표는 월드 기준이라 플레이어가 움직여도 쏜 자리에 그대로 남는다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class LightFlash : MonoBehaviour
{
    [Header("시간")]
    [SerializeField] private float _duration = 0.18f;
    [Tooltip("클수록 터진 직후 급격히 꺼지고 잔광만 길게 남는다. 1이면 일정하게 옅어진다")]
    [SerializeField, Min(1f)] private float _falloff = 3f;

    [Header("모습")]
    [SerializeField] private float _width = 0.45f;      // 터지는 순간 굵기 (조준선은 0.15)
    [SerializeField] private float _intensity = 10f;    // 터지는 순간 HDR 밝기 (조준선은 1~1.5)
    [SerializeField] private Color _color = new Color(1f, 1f, 0f, 0.9f);

    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

    private LineRenderer _line;
    private Material _material; // 머티리얼 인스턴스 (원본 .mat과 조준선 쪽에 영향을 주지 않는다)
    private readonly List<Vector3> _points = new List<Vector3>();
    private float _elapsed;
    private bool _playing;

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.useWorldSpace = true; // 플레이어를 따라다니지 않고 쏜 자리에 남는다
        _material = _line.material;
        _line.enabled = false;
    }

    private void OnDestroy()
    {
        if (_material != null) Destroy(_material);
    }

    /// <summary>발사 위치에서 경로를 따라 번쩍인다. 이전 플래시가 남아 있으면 덮어쓴다.</summary>
    public void Play(Vector2 from, IReadOnlyList<Vector2> path)
    {
        if (_duration <= 0f || path == null || path.Count == 0) return;

        _points.Clear();
        _points.Add(from);
        for (int i = 0; i < path.Count; i++) _points.Add(path[i]);

        _line.positionCount = _points.Count;
        for (int i = 0; i < _points.Count; i++) _line.SetPosition(i, _points[i]);

        _line.startColor = _color;
        _line.endColor = _color;
        _line.enabled = true;

        _elapsed = 0f;
        _playing = true;
        Apply(0f);
    }

    private void Update()
    {
        if (!_playing) return;

        // Time.deltaTime을 쓰므로 히트스톱으로 멈춘 동안에는 플래시도 멈춰 있는다
        _elapsed += Time.deltaTime;

        if (_elapsed >= _duration)
        {
            Stop();
            return;
        }

        Apply(_elapsed / _duration);
    }

    // t는 0(막 터진 순간) ~ 1(다 사그라든 순간)
    private void Apply(float t)
    {
        float k = Mathf.Pow(1f - Mathf.Clamp01(t), _falloff);

        _line.startWidth = _width * k;
        _line.endWidth = _width * k;
        if (_material != null) _material.SetFloat(IntensityId, _intensity * k);
    }

    public void Stop()
    {
        _playing = false;
        _line.positionCount = 0;
        _line.enabled = false;
    }
}
