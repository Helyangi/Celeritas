using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 연출을 건너뛰는 입력을 한 곳에서 판정한다. (마우스 클릭, 아무 키, 패드 A/Start)
/// 스플래시를 건너뛰었다는 사실도 여기에 남겨서, 다음 씬(메인 메뉴)이 기다리지 않고 바로 떠오르게 한다.
/// </summary>
public static class SkipInput
{
    private static bool _introSkipped;

    // 플레이 모드를 다시 시작해도 static 값이 남지 않도록 초기화한다
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _introSkipped = false;
    }

    /// <summary>이번 프레임에 건너뛰기 입력이 들어왔는가.</summary>
    public static bool Pressed()
    {
        Mouse mouse = Mouse.current;
        if (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)) return true;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.anyKey.wasPressedThisFrame) return true;

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && (gamepad.buttonSouth.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame)) return true;

        return false;
    }

    /// <summary>스플래시를 건너뛰었다고 표시한다.</summary>
    public static void MarkIntroSkipped()
    {
        _introSkipped = true;
    }

    /// <summary>스플래시를 건너뛰었는지 확인하고 표시를 지운다. (한 번만 쓰이도록)</summary>
    public static bool ConsumeIntroSkipped()
    {
        bool skipped = _introSkipped;
        _introSkipped = false;
        return skipped;
    }
}
