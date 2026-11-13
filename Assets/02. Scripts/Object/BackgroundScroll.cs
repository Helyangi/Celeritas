using UnityEngine;

// 정지 화면(메인 메뉴 등)에서 배경 텍스처를 시간에 따라 천천히 흘려보낸다.
// Background.cs는 오브젝트가 움직일 때만 스크롤되는 패럴랙스용이라, 아무것도 움직이지 않는 화면에서는 이 스크립트를 쓴다.
// 스프라이트는 Wrap Mode = Repeat, Mesh Type = Full Rect여야 하고 아틀라스에 포함되면 안 된다.
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScroll : MonoBehaviour
{
    [SerializeField] private Vector2 _speed = new Vector2(0.01f, 0f); // 초당 UV 이동량 (0.005 ~ 0.02 정도가 은은하다)

    private Material _material; // SpriteRenderer.material은 복사본이라 원본 .mat 파일은 건드리지 않는다
    private Vector2 _offset;

    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
        _offset = _material.mainTextureOffset;
    }

    private void Update()
    {
        _offset += _speed * Time.unscaledDeltaTime; // 메뉴에서 timeScale이 0이어도 계속 흐르게 한다
        _offset.x = Mathf.Repeat(_offset.x, 1f);    // 값이 무한히 커져 정밀도가 깨지는 것을 막는다
        _offset.y = Mathf.Repeat(_offset.y, 1f);
        _material.mainTextureOffset = _offset;
    }

    private void OnDestroy()
    {
        if (_material != null) Destroy(_material); // 복사본 정리
    }
}
