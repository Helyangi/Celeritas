using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임을 켜면 가장 먼저 나오는 씬에서 팀명을 서서히 띄웠다 지우고, 다음 씬으로 넘어간다.
/// 팀명이 보이는 동안 다음 씬을 미리 불러두고, 사라진 뒤에 전환한다.
/// </summary>
public class TeamSplash : MonoBehaviour
{
    [SerializeField] private CanvasGroup _group; // 팀명의 투명도

    [Header("시간")]
    [SerializeField] private float _startDelay = 0.3f;     // 켜지자마자 바로 뜨지 않도록 잠깐 기다린다
    [SerializeField] private float _fadeInDuration = 1f;   // 떠오르는 시간
    [SerializeField] private float _holdDuration = 1.6f;   // 떠 있는 시간
    [SerializeField] private float _fadeOutDuration = 0.8f; // 사라지는 시간

    [Header("다음 씬")]
    [SerializeField] private string _nextScene = "MainMenu";

    private void Start()
    {
        if (_group != null) _group.alpha = 0f;
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        yield return new WaitForSeconds(_startDelay);

        // 팀명이 보이는 동안 다음 씬을 미리 불러둔다 (다 불러와도 바로 넘어가지 않게 막아둔다)
        AsyncOperation load = string.IsNullOrEmpty(_nextScene) ? null : SceneManager.LoadSceneAsync(_nextScene);
        if (load != null) load.allowSceneActivation = false;

        yield return Fade(1f, _fadeInDuration);
        yield return new WaitForSeconds(_holdDuration);
        yield return Fade(0f, _fadeOutDuration);

        if (load != null) load.allowSceneActivation = true;
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
