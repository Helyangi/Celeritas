using System.Collections.Generic;
using UnityEngine;

public class LightAimingLine : MonoBehaviour
{
    public LayerMask ContactLayer; // 레이가 닿는 레이어 (거울, 빛 구체, 벽 등을 모두 포함해야 함)
    public LayerMask MirrorLayer; // ContactLayer 중에서 "거울"에 해당하는 레이어
    public int MaxReflection = 10; // 최대 반사 횟수
    public float MaxDistance = 40; // 조준선 최대 길이 (반사된 구간까지 합친 전체 길이)
    [SerializeField] private Color _heatingColor;
    [SerializeField] private Color _coolingColor;
    private LineRenderer _lineRenderer;
    
    public void Awake()
    {
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>(); // 예외 처리

        _lineRenderer.startWidth = 0.2f;
        _lineRenderer.endWidth = 0.2f;
        _lineRenderer.numCornerVertices = 5; // 거울에서 꺾이는 부분을 둥글게 이어줌
    }
    public void Start()
    {
        
    }
    public void DrawAimingLine(List<Vector3> points) // 게임에서 조준선 그려주는 함수 (점 리스트를 순서대로 이어서 그림)
    {
        _lineRenderer.positionCount = points.Count; // 이번 프레임에 쓴 점 개수만큼만 그린다
        for (int i = 0; i < points.Count; i++)
        {
            _lineRenderer.SetPosition(i, points[i]);
        }
    }
    public bool IsMirror(Collider2D col) // 이 콜라이더가 거울 레이어인지 판단하는 함수
    {
        return col.gameObject.layer == LayerMask.NameToLayer("Mirror");
    }
    
    public void HeatingColor()
    {
        _lineRenderer.startColor = _heatingColor;
        _lineRenderer.endColor = _heatingColor;
    }
    
    public void CoolingColor()
    {
        _lineRenderer.startColor = _coolingColor;
        _lineRenderer.endColor = _coolingColor;
    }
}
