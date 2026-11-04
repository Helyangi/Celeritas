using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private RailPath rail;

    // 목표 위치
    private Vector3 _goalPoint;
    
    private void Start()
    {
        if (rail == null) return;

        _goalPoint = rail.StartPoint;
    }
    
    private void Update()
    {
        if (rail == null) return;   
        
        if (transform.position == _goalPoint)
        {
            _goalPoint = _goalPoint == rail.StartPoint ? rail.EndPoint : rail.StartPoint;
        }
        
        Vector3 newPosition = Vector3.MoveTowards(
            transform.position,
            _goalPoint,
            speed * Time.deltaTime);
        transform.position = newPosition;
    }
}
