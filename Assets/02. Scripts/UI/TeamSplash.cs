using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임을 켜면 가장 먼저 나오는 씬에서 팀명을 서서히 띄웠다 지우고, 다음 씬으로 넘어간다.
/// 팀명이 보이는 동안 다음 씬을 미리 불러두고, 사라진 뒤에 전환한다.
/// 클릭이나 아무 키를 누르면 건너뛴다.
/// </summary>
public class TeamSplash : MonoBehaviour
{
    [SerializeField] private CanvasGroup _group; // 팀명의 투명도

    [Header("시간")]
    [SerializeField] private float _startDelay = 0.3f;     // 켜지자마자 바로 뜨지 않도록 잠깐 기다린다
    [SerializeField] private float _fadeInDuration = 1f;   // 떠오르는 시간
    [SerializeField] private float _holdDuration = 1.6f;   // 떠 있는 시간
    [SerializeField] private float _fadeOutDuration = 0.8f; // 사라지는 시간

    [Header("건너뛰기")]
    [Tooltip("클릭이나 아무 키로 건너뛸 수 있게 한다")]
    [SerializeField] private bool _skippable = true;
    [Tooltip("건너뛸 때 팀명이 사라지는 시간(초). 0이면 툭 끊긴다")]
    [SerializeField, Min(0f)] private float _skipFadeDuration = 0.2f;

    [Header("다음 씬")]
    [SerializeField] private string _nextScene = "MainMenu";

    private AsyncOperation _load;
    private Coroutine _routine;
    private bool _skipping;

    private void Start()
    {
        if (_group != null) _group.alpha = 0f;
        _routine = StartCoroutine(Routine());
    }

    private void Update()
    {
        if (!_skippable || _skipping) return;
        if (SkipInput.Pressed()) Skip();
    }

    private IEnumerator Routine()
    {
        yield return new WaitForSeconds(_startDelay);

        // 팀명이 보이는 동안 다음 씬을 미리 불러둔다 (다 불러와도 바로 넘어가지 않게 막아둔다)
        BeginLoad();

        yield return Fade(1f, _fadeInDuration);
        yield return new WaitForSeconds(_holdDuration);
        yield return Fade(0f, _fadeOutDuration);

        Activate();
    }

    private void Skip()
    {
        _skipping = true;
        if (_routine != null) StopCoroutine(_routine);
        StartCoroutine(SkipRoutine());
    }

    private IEnumerator SkipRoutine()
    {
        BeginLoad(); // 시작 대기 중에 눌렀다면 아직 불러오기 전이다

        yield return Fade(0f, _skipFadeDuration); // 지금 보이는 만큼에서 빠르게 지운다

        SkipInput.MarkIntroSkipped(); // 메인 메뉴도 기다리지 말고 바로 떠오르게 알린다
        Activate();
    }

    private void BeginLoad()
    {
        if (_load != null || string.IsNullOrEmpty(_nextScene)) return;

        _load = SceneManager.LoadSceneAsync(_nextScene);
        if (_load != null) _load.allowSceneActivation = false;
    }

    private void Activate()
    {
        if (_load != null) _load.allowSceneActivation = true;
    }

    private IEnumerator Fade(float target, float duration)
    {
        if (_group == null) yield break;

        if (duration <= 0f)
        {
            _group.alpha = target;
            yield break;
        }

        float start = _group.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        _group.alpha = target;
    }
}
