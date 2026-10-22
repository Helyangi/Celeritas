using UnityEngine;

public class Background : MonoBehaviour
{
    [SerializeField] private Material _material;
    [SerializeField] private float _speed = 0.1f;
    private Vector3 _initialPos;
    private void Awake()
    {
        _initialPos = transform.position;
    }
    private void Update()
    {
        Vector2 offset = (transform.position - _initialPos) * _speed;
        _material.mainTextureOffset = offset;
    }
}