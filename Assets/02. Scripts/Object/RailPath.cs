using UnityEngine;

public class RailPath : MonoBehaviour
{
    [SerializeField] private float length;
    
    public Vector3 StartPoint =>
        transform.TransformPoint(-0.5f, 0, 0);

    public Vector3 EndPoint =>
        transform.TransformPoint(0.5f, 0, 0);

    void Awake()
    {
        if (length == 0) return;
        
        // 늘리기
        Vector3 scale = transform.localScale; 
        scale.x = length; 
        transform.localScale = scale;
    }
}
