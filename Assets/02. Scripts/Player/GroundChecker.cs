using System;
using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private Transform _point;
    [SerializeField] private Vector2 _size;
    [SerializeField] private Color _color;
    public bool IsGround()
    {
        return Physics2D.OverlapBox(_point.position, _size, 0, GroundLayer);
    }
    private void OnDrawGizmos()
    {
        if (_point == null) return;
        Gizmos.color = _color;
        Gizmos.DrawWireCube(_point.position, _size);
    }
}