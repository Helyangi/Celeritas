using UnityEngine;

/// <summary>
/// 컷씬을 시작시키는 장치.
/// - OnSceneStart: 씬이 시작되면 바로 재생 (인트로 씬 등)
/// - OnPlayerEnter: 플레이어가 이 오브젝트의 트리거 영역에 들어오면 재생 (스테이지 중간 컷씬 등, Collider2D의 Is Trigger 필요)
/// 다른 스크립트나 UI 버튼에서 Play()를 직접 불러도 된다.
/// </summary>
public class CutsceneTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        [InspectorName("씬 시작 시")] OnSceneStart,
        [InspectorName("플레이어가 들어오면")] OnPlayerEnter
    }

    [SerializeField] private CutsceneData _cutscene;
    [SerializeField] private TriggerType _triggerType = TriggerType.OnSceneStart;
    [Tooltip("한 번 재생하면 다시 재생하지 않는다 (씬을 다시 불러오면 초기화된다)")]
    [SerializeField] private bool _playOnce = true;

    private bool _played;

    private void Start()
    {
        if (_triggerType == TriggerType.OnSceneStart) Play();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggerType != TriggerType.OnPlayerEnter) return;
        if (!other.CompareTag("Player")) return;

        Play();
    }

    public void Play()
    {
        if (_playOnce && _played) return;

        if (CutsceneManager.Instance == null)
        {
            Debug.LogWarning("[CutsceneTrigger] 씬에 CutsceneManager가 없어서 컷씬을 재생하지 못했습니다.", this);
            return;
        }

        if (CutsceneManager.Instance.Play(_cutscene)) _played = true;
    }
}
