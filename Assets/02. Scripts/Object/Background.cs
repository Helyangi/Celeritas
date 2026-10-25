using UnityEngine;

public class Background : MonoBehaviour
{
    private Material _material;
    [SerializeField] private float _speed = 0.1f;
    private Vector3 _initialPos;
    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
        _initialPos = transform.position;
    }
    private void Update()
    {
        Vector2 offset = (transform.position - _initialPos) * _speed;
        _material.mainTextureOffset = offset;
    }
    private void OnDestroy()
    {
        if (_material != null) Destroy(_material); // 복사본 정리
    }
}