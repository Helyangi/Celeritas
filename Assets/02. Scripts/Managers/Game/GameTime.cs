using UnityEngine;

/// <summary>
/// 게임 시간(Time.timeScale)의 유일한 주인. 다른 코드는 timeScale을 직접 쓰지 않고 여기를 거친다.
/// 각자 자기 스위치만 바꾸고, 실제 값은 바뀔 때마다 처음부터 다시 계산한다.
/// 아무도 "원래 값"을 기억했다가 되돌리지 않기 때문에, 서로 겹쳐도 엉키지 않는다.
///
/// 규칙: 일시정지가 항상 이긴다. 그 외 시간 연출(슬로우모션 등)이 생기면 스위치를 하나 늘리고 Apply에서 곱한다.
/// </summary>
public static class GameTime
{
    private static bool _paused;
    private static float _hitstopScale = 1f;

    public static bool IsPaused => _paused;

    // 플레이 모드를 다시 시작해도 static 값이 남지 않도록 초기화한다
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _paused = false;
        _hitstopScale = 1f;
    }

    /// <summary>일시정지 스위치. 켜져 있는 동안은 다른 스위치와 상관없이 시간이 멈춘다.</summary>
    public static void SetPaused(bool value)
    {
        _paused = value;
        Apply();
    }

    /// <summary>히트스톱 스위치. 끝날 때는 1로 "내 몫을 내려놓는다" (되돌리기가 아니다).</summary>
    public static void SetHitstopScale(float scale)
    {
        _hitstopScale = Mathf.Clamp01(scale);
        Apply();
    }

    private static void Apply()
    {
        Time.timeScale = _paused ? 0f : _hitstopScale;
    }
}
