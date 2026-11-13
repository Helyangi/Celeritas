using UnityEngine;

/// <summary>
/// 씬마다 하나씩 두고 그 씬에서 울릴 배경음악을 지정한다.
/// 이미 같은 곡이 흐르고 있으면 끊지 않고 이어서 재생되고, 다른 곡이면 페이드로 바뀐다.
/// 클립을 비워 두면 그 씬에서는 음악을 끈다.
/// </summary>
public class SceneBGM : MonoBehaviour
{
    [SerializeField] private AudioClip _clip;          // 비워 두면 음악 끄기
    [SerializeField] private float _fadeDuration = 1f; // 곡이 바뀔 때 겹치는 시간(초)

    private void Start()
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[SceneBGM] SoundManager가 없어서 배경음악을 바꾸지 못했습니다.", this);
            return;
        }

        SoundManager.Instance.PlayBGM(_clip, _fadeDuration);
    }
}
