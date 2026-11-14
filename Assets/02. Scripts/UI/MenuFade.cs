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
    [Tooltip("씬이 시작되고 페이드 인이 시작되기까지 기다리는 시간(초). 검은 화면이 잠깐 유지된다")]
    [SerializeField, Min(0f)] private float _fadeInDelay = 0.5f;
    [SerializeField, Min(0f)] private float _fadeInDuration = 0.8f;  // 메뉴가 떠오르는 시간
    [SerializeField, Min(0f)] private float _fadeOutDuration = 0.5f; // 메뉴를 떠나며 어두워지는 시간

    [Header("건너뛰기")]
    [Tooltip("페이드 인 도중 클릭이나 아무 키로 건너뛸 수 있게 한다")]
    [SerializeField] private bool _skippable = true;
    [Tooltip("건너뛸 때(또는 스플래시를 건너뛰고 들어왔을 때) 떠오르는 시간(초)")]
    [SerializeField, Min(0f)] private float _skipFadeDuration = 0.3f;

    private bool _leaving; // 페이드 아웃 도중 버튼을 또 눌러도 두 번 넘어가지 않게 한다
    private Coroutine _fadeInRoutine;
    private bool _fadingIn;
    private bool _fadeInSkipped;

    /// <summary>메뉴를 떠나는 중인가. 버튼들은 이 동안 상태를 얼려둔다.</summary>
    public bool IsLeaving => _leaving;

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

        // 스플래시를 건너뛰고 들어왔으면 메뉴도 기다리게 하지 않는다 (한 번 눌렀는데 또 기다리면 건너뛴 의미가 없다)
        if (SkipInput.ConsumeIntroSkipped())
        {
            _fadeInSkipped = true;
            _fadeInRoutine = StartCoroutine(FadeInRoutine(0f, _skipFadeDuration));
        }
        else
        {
            _fadeInRoutine = StartCoroutine(FadeInRoutine(_fadeInDelay, _fadeInDuration));
        }
    }

    // 잠깐 검은 화면을 유지했다가 떠오른다. 바로 밝아지면 급해 보인다.
    private IEnumerator FadeInRoutine(float delay, float duration)
    {
        _fadingIn = true;
        _fader.SetBlockInput(true); // 아직 덮여 있는 동안에는 버튼이 눌리지 않게

        if (delay > 0f) yield return new WaitForSeconds(delay);

        yield return _fader.FadeTo(0f, duration); // 지금 투명도에서 시작하므로 중간에 건너뛰어도 튀지 않는다

        _fader.SetBlockInput(false); // 다 떠오르면 조작을 넘겨준다
        _fadingIn = false;
        _fadeInRoutine = null;
    }

    private void Update()
    {
        if (!_fadingIn || _fadeInSkipped || !_skippable || _leaving) return;
        if (SkipInput.Pressed()) SkipFadeIn();
    }

    // 남은 대기와 페이드를 버리고 빠르게 떠오른다.
    // 건너뛰는 클릭은 아직 막이 입력을 막고 있어서 뒤쪽 버튼까지 눌리지 않는다.
    private void SkipFadeIn()
    {
        _fadeInSkipped = true;
        if (_fadeInRoutine != null) StopCoroutine(_fadeInRoutine);
        _fadeInRoutine = StartCoroutine(FadeInRoutine(0f, _skipFadeDuration));
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

        // 떠나는 중에는 더 이상 입력을 받지 않는다 (호버가 바뀌거나 다른 버튼이 눌리면 이상하다)
        if (_fader != null) _fader.SetBlockInput(true);

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
