using System.Collections;
using UnityEngine;

/// <summary>
/// 아주 짧은 순간 시간을 멈춰 타격감을 만든다 (히트스톱).
/// 어디서든 Hitstop.Play(0.05f)로 부르면 되고, 씬에 미리 둘 것은 없다 (처음 부를 때 스스로 만들어진다).
/// 멈춰 있는 동안에도 흐르는 시간(unscaledDeltaTime)으로 세기 때문에 정확히 그 길이만큼만 멈춘다.
/// 시간은 직접 바꾸지 않고 GameTime의 히트스톱 스위치만 만진다 (일시정지와 엉키지 않게).
/// </summary>
public static class Hitstop
{
    private static Runner _runner;
    private static float _remaining;
    private static bool _running;

    public static bool IsRunning => _running;

    // 플레이 모드를 다시 시작해도 static 값이 남지 않도록 초기화한다
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _runner = null;
        _running = false;
        _remaining = 0f;
    }

    /// <summary>
    /// duration초(실시간) 동안 시간을 멈춘다. timeScale에 0보다 큰 값을 주면 완전히 멈추는 대신 느려진다.
    /// </summary>
    public static void Play(float duration, float timeScale = 0f)
    {
        if (!Application.isPlaying || duration <= 0f) return;
        if (GameTime.IsPaused) return; // 일시정지 중에는 걸지 않는다

        if (_running)
        {
            _remaining = Mathf.Max(_remaining, duration); // 겹쳐 부르면 더 긴 쪽에 맞춘다 (길이가 쌓이지 않는다)
            return;
        }

        EnsureRunner();
        _remaining = duration;
        _runner.Begin(Mathf.Clamp01(timeScale));
    }

    private static void EnsureRunner()
    {
        if (_runner != null) return;

        GameObject go = new GameObject("@Hitstop");
        Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<Runner>();
    }

    // 코루틴을 돌리기 위한 최소한의 MonoBehaviour
    private class Runner : MonoBehaviour
    {
        public void Begin(float scale)
        {
            StopAllCoroutines();
            StartCoroutine(Routine(scale));
        }

        private IEnumerator Routine(float scale)
        {
            _running = true;
            GameTime.SetHitstopScale(scale);

            while (_remaining > 0f)
            {
                _remaining -= Time.unscaledDeltaTime;
                yield return null;
            }

            // "원래 값으로 되돌리기"가 아니라 "내 몫을 내려놓기". 일시정지 중이면 여전히 멈춰 있는다
            GameTime.SetHitstopScale(1f);
            _running = false;
        }

        // 멈춘 채로 오브젝트가 사라져 히트스톱 스위치가 내려간 채로 남지 않게 한다
        private void OnDisable()
        {
            if (!_running) return;

            GameTime.SetHitstopScale(1f);
            _running = false;
            _remaining = 0f;
        }
    }
}
