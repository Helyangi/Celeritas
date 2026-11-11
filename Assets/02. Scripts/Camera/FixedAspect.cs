using UnityEngine;
using UnityEngine.UI;

// 화면 비율을 고정한다. 모니터가 목표 비율(기본 16:9)보다 가로로 넓으면 좌우에 검은 띠를 덮어서
// 어떤 모니터에서도 보이는 가로 범위가 같게 만든다. (와이드 모니터라서 더 멀리 보이는 일이 없어진다)
//
// 카메라 영역(Camera.rect)으로 자르는 방식은 URP 2D에서 후처리(블룸)가 켜져 있으면 위치 이동이 적용되지 않아서,
// 카메라는 그대로 두고 그 위에 검은 띠를 덮는 방식을 쓴다. 가운데 16:9 영역은 원래 화면과 똑같이 보인다.
// 목표보다 좁은 화면(4:3, 16:10 등)은 보이는 범위가 오히려 줄어드는 쪽이라 따로 처리하지 않는다.
// 띠는 실행할 때 자동으로 만들고, UI 캔버스보다 아래에 그려서 UI는 띠 위에서도 보인다.
public class FixedAspect : MonoBehaviour
{
    [SerializeField] private float _targetWidth = 16f;
    [SerializeField] private float _targetHeight = 9f;
    [SerializeField] private int _sortingOrder = -100; // 다른 UI 캔버스보다 낮게 둔다

    private RectTransform _left;
    private RectTransform _right;
    private int _screenWidth;
    private int _screenHeight;

    private void Awake()
    {
        CreateBars();
        Apply();
    }

    private void Update()
    {
        // 창 크기나 전체화면 전환 등으로 해상도가 바뀐 경우에만 다시 계산한다
        if (Screen.width != _screenWidth || Screen.height != _screenHeight) Apply();
    }

    private void Apply()
    {
        _screenWidth = Screen.width;
        _screenHeight = Screen.height;
        if (_screenWidth <= 0 || _screenHeight <= 0) return; // 최소화 상태

        ApplyAspect((float)_screenWidth / _screenHeight);
    }

    public void ApplyAspect(float screenAspect)
    {
        float fraction = BarFraction(screenAspect, _targetWidth / _targetHeight);

        _left.gameObject.SetActive(fraction > 0f);
        _right.gameObject.SetActive(fraction > 0f);

        SetBar(_left, 0f, fraction);
        SetBar(_right, 1f - fraction, 1f);
    }

    // 화면 가로 폭에서 한쪽 띠가 차지하는 비율 (0이면 띠 없음)
    public static float BarFraction(float screenAspect, float targetAspect)
    {
        if (screenAspect <= targetAspect) return 0f;
        return (1f - targetAspect / screenAspect) * 0.5f;
    }

    private static void SetBar(RectTransform bar, float minX, float maxX)
    {
        bar.anchorMin = new Vector2(minX, 0f);
        bar.anchorMax = new Vector2(maxX, 1f);
        bar.offsetMin = Vector2.zero;
        bar.offsetMax = Vector2.zero;
    }

    private void CreateBars()
    {
        GameObject canvasObject = new GameObject("LetterboxBars");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = _sortingOrder;

        _left = CreateBar("Left", canvasObject.transform);
        _right = CreateBar("Right", canvasObject.transform);
    }

    private static RectTransform CreateBar(string name, Transform parent)
    {
        GameObject bar = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        bar.transform.SetParent(parent, false);

        Image image = bar.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false; // 마우스 입력을 막지 않는다

        return (RectTransform)bar.transform;
    }
}