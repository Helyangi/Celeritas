using UnityEngine;

public class WallChecker : MonoBehaviour
{
    [SerializeField] private Transform _rightPoint;
    [SerializeField] private Transform _leftPoint;
    [SerializeField] private Vector2 _size;
    [SerializeField] private Color _color;

    public int GetWallDirection()
    {
        if (CheckWall(_leftPoint)) return -1;
        if (CheckWall(_rightPoint)) return 1;
        return 0;
    }

    private bool CheckWall(Transform point)
    {
        return Physics2D.OverlapBox(
            point.position,
            _size,
            0,
            LayerMask.GetMask("Wall")
        );
    }

    private void OnDrawGizmos()
    {
        if (_leftPoint == null || _rightPoint == null) return;
        
        Gizmos.color = _color;
        Gizmos.DrawWireCube(_rightPoint.position, _size);
        Gizmos.DrawWireCube(_leftPoint.position, _size);
    }
}
