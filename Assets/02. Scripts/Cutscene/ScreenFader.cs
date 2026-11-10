using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화면 전체를 덮는 가림막. 페이드 단계가 이걸로 화면을 어둡게/밝게 만든다.
/// 화면을 꽉 채우는 Image + CanvasGroup 오브젝트에 붙인다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Image _image; // 가림막 색을 바꿀 때 사용
    private CanvasGroup _group;

    public float Alpha => Group.alpha;

    private CanvasGroup Group
    {
        get
        {
            if (_group == null) _group = GetComponent<CanvasGroup>();
            return _group;
        }
    }

    private void Awake()
    {
        if (_image == null) _image = GetComponent<Image>();
        Group.blocksRaycasts = false; // 가림막이 클릭을 막지 않도록
        Group.interactable = false;
    }

    public void SetAlpha(float alpha)
    {
        Group.alpha = Mathf.Clamp01(alpha);
    }

    public void SetColor(Color color)
    {
        if (_image == null) return;
        color.a = 1f; // 투명도는 CanvasGroup이 담당한다
        _image.color = color;
    }

    /// <summary>
    /// 지금 투명도에서 target까지 duration초 동안 바꾼다.
    /// </summary>
    public IEnumerator FadeTo(float target, float duration)
    {
        target = Mathf.Clamp01(target);
        float start = Group.alpha;

        if (duration <= 0f)
        {
            Group.alpha = target;
            yield break;
        }

        float time = 0f;
        while (time < duration)
        {
            // Time.deltaTime은 한 프레임 최대치가 정해져 있어서, 씬 로딩 직후처럼 프레임이 튀어도 페이드가 건너뛰어지지 않는다
            time += Time.deltaTime;
            Group.alpha = Mathf.Lerp(start, target, time / duration);
            yield return null;
        }
        Group.alpha = target;
    }
}
