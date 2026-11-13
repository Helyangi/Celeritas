using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인 메뉴 전용 연출 담당. 씬이 시작되면 덮여 있던 화면을 걷어내고(페이드 인),
/// 메뉴를 떠날 때는 화면이 어두워진 뒤에(페이드 아웃) 씬으로 이동한다.
/// 가림막은 컷씬과 같은 ScreenFader를 쓴다. 앞으로 메뉴에서만 필요한 기능은 여기에 모은다.
/// </summary>
public class MenuFade : MonoBehaviour
{
    public static MenuFade Instance { get; private set; }

    [SerializeField] private ScreenFader _fader; // 화면을 꽉 채우는 Image + CanvasGroup 오브젝트

    [Header("색")]
    [SerializeField] private Color _color = Color.black;

    [Header("시간")]
    [SerializeField, Min(0f)] private float _fadeInDuration = 0.8f;  // 메뉴가 떠오르는 시간
    [SerializeField, Min(0f)] private float _fadeOutDuration = 0.5f; // 메뉴를 떠나며 어두워지는 시간

    private bool _leaving; // 페이드 아웃 도중 버튼을 또 눌러도 두 번 넘어가지 않게 한다

    private void Awake()
    {
        Instance = this;
        if (_fader == null) _fader = GetComponentInChildren<ScreenFader>(true); // 연결을 깜빡해도 자식에서 찾아본다
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (_fader == null)
        {
            Debug.LogWarning("[MenuFade] ScreenFader가 연결되어 있지 않아 페이드를 건너뜁니다.");
            return;
        }

        _fader.SetColor(_color);
        _fader.SetAlpha(1f); // 페이드 인은 항상 완전히 덮인 화면에서 시작한다
        StartCoroutine(_fader.FadeTo(0f, _fadeInDuration));
    }

    /// <summary>
    /// 화면이 어두워진 뒤에 해당 씬으로 이동한다. 버튼에서 직접 부를 수 있다.
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[MenuFade] 이동할 씬 이름이 비어 있습니다.");
            return;
        }

        FadeOutThen(() => SceneManager.LoadScene(sceneName));
    }

    /// <summary>
    /// 화면이 어두워진 뒤에 주어진 일을 한다. (씬 이동 외의 마무리 처리에도 쓸 수 있다)
    /// </summary>
    public void FadeOutThen(Action action)
    {
        if (_leaving) return;
        _leaving = true;

        StartCoroutine(FadeOutRoutine(action));
    }

    private IEnumerator FadeOutRoutine(Action action)
    {
        if (_fader != null)
        {
            _fader.SetColor(_color);
            yield return _fader.FadeTo(1f, _fadeOutDuration);
        }

        action?.Invoke();
    }
}
