using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어가 죽으면 화면을 어둡게 덮고, 다시 등장하면 걷어낸다.
/// 어느 스테이지에서든 쓸 수 있도록 씬이 바뀌어도 유지되며, 같은 것이 둘이면 나중 것이 스스로 사라진다.
/// 덮는 정도와 시간은 인스펙터에서 조절한다.
/// </summary>
public class ScreenFade : MonoBehaviour
{
    public static ScreenFade Instance { get; private set; }

    [SerializeField] private CanvasGroup _group; // 이 값의 투명도를 바꿔서 화면을 덮는다

    [Header("덮는 정도")]
    [Range(0f, 1f)]
    [SerializeField] private float _maxAlpha = 1f; // 1이면 완전히 가려지고, 0.7이면 맵이 희미하게 비친다

    [Header("시간")]
    [SerializeField] private float _fadeOutDelay = 0.15f;   // 죽는 연출이 보이도록 조금 늦게 깔린다
    [SerializeField] private float _fadeOutDuration = 0.5f; // 어두워지는 시간
    [SerializeField] private float _fadeInDuration = 0.6f;  // 등장하면서 걷히는 시간

    private PlayerController _player;
    private Coroutine _routine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (_group == null) _group = GetComponent<CanvasGroup>();
        if (_group != null) _group.alpha = 0f;
    }

    private void OnEnable()
    {
        if (Instance != this) return;

        PlayerController.Spawned += OnSpawned;
    }

    private void OnDisable()
    {
        PlayerController.Spawned -= OnSpawned;
        Unhook();
    }

    private void Start()
    {
        if (Instance != this) return;

        Hook(FindFirstObjectByType<PlayerController>()); // 이미 등장해 있는 플레이어도 잡아둔다
    }

    // 등장(게임 시작·리스폰): 그 플레이어의 죽음을 구독하고, 덮여 있던 화면을 걷어낸다
    private void OnSpawned(Transform player, Vector3 delta)
    {
        Hook(player != null ? player.GetComponent<PlayerController>() : null);
        StartFade(0f, _fadeInDuration, 0f);
    }

    private void OnDied()
    {
        StartFade(_maxAlpha, _fadeOutDuration, _fadeOutDelay);
    }

    private void Hook(PlayerController player)
    {
        if (player == null || _player == player) return;

        Unhook();
        _player = player;
        _player.Died += OnDied;
    }

    private void Unhook()
    {
        if (_player == null) return;

        _player.Died -= OnDied;
        _player = null;
    }

    private void StartFade(float target, float duration, float delay)
    {
        if (_group == null) return;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FadeRoutine(target, duration, delay));
    }

    private IEnumerator FadeRoutine(float target, float duration, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (duration <= 0f)
        {
            _group.alpha = target;
            _routine = null;
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
        _routine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
