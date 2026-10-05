using UnityEngine;

public class LightAimingLine : MonoBehaviour
{
    public LayerMask ContactLayer;
    public float MaxDistance = 40;
    
    [SerializeField] private Color _heatingColor;
    [SerializeField] private Color _coolingColor;
    private LineRenderer _lineRenderer;
    
    public void Awake()
    {
        if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>(); // 예외 처리

        _lineRenderer.startWidth = 0.2f;
        _lineRenderer.endWidth = 0.2f;
    }
    
    public void DrawAimingLine(Vector2 startPos, Vector2 endPos) // 게임에서 조준선 그려주는 함수
    {
        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);
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
