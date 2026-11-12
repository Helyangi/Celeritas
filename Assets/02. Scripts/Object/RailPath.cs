using UnityEngine;

public class RailPath : MonoBehaviour
{
    [SerializeField] private float length;
    
    public Vector3 StartPoint =>
        transform.TransformPoint(-0.5f * length, 0, 0);

    public Vector3 EndPoint =>
        transform.TransformPoint(0.5f * length, 0, 0);

    void Awake()
    {
        if (length == 0) return;
        
        // 늘리기
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(length, 0.3f);
    }

    private void OnValidate()
    {
        if (length <= 0) return;
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(length, 0.3f);
    }
}
