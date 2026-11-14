using UnityEngine;

/// <summary>
/// 게임을 종료한다.
/// 에디터에서는 플레이 모드를 멈춘다 (빌드가 아니라 Application.Quit이 동작하지 않기 때문).
/// </summary>
public class QuitButton : MonoBehaviour
{
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
