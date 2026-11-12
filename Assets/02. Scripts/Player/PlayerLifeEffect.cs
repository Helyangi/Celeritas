using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 죽음/등장 연출을 담당한다.
/// PlayerController의 이벤트(Died, Spawned, SpawnFinished)만 구독하며, 다른 곳에서 이 스크립트를 참조하지 않는다.
/// 죽을 때는 Die 애니메이션을 정방향으로, 등장할 때는 같은 애니메이션을 역방향으로 재생한다.
/// (애니메이터의 Die 상태 Speed Multiplier가 DieSpeed 파라미터에 연결되어 있다)
/// 같은 타이밍에 플레이어의 빛도 서서히 꺼지고 켜진다.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerLifeEffect : MonoBehaviour
{
    private static readonly int DieSpeedId = Animator.StringToHash("DieSpeed");
    private const string DieState = "Die";
    private const string IdleState = "Idle";

    [Header("빛 연출")]
    [SerializeField] private float _fadeOutDuration = 0.5f; // 죽을 때 빛이 꺼지는 시간 (등장 때 켜지는 시간은 PlayerController의 Spawn Duration을 따른다)

    private PlayerController _player;
    private float _baseIntensity; // 평소 빛 밝기
    private Coroutine _routine;
    private float _dieClipLength = 0.77f; // Die 클립 길이 (Awake에서 애니메이터에서 읽는다)

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        if (_player.Light != null) _baseIntensity = _player.Light.intensity;

        // 등장 시간이 클립보다 짧으면 역재생이 끝나기 전에 조작이 풀리므로, 길이를 알아둔다
        if (_player.Anim != null && _player.Anim.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in _player.Anim.runtimeAnimatorController.animationClips)
            {
                if (clip.name == DieState) { _dieClipLength = clip.length; break; }
            }
        }
    }

    private void OnEnable()
    {
        _player.Died += OnDied;
        _player.SpawnFinished += OnSpawnFinished;
        PlayerController.Spawned += OnSpawned;
    }

    private void OnDisable()
    {
        _player.Died -= OnDied;
        _player.SpawnFinished -= OnSpawnFinished;
        PlayerController.Spawned -= OnSpawned;

        StopFade(); // 꺼지는 도중에 비활성화되면 연출만 멈춘다
    }

    // 죽는 순간: Die 애니메이션을 처음부터 정방향으로 재생하고, 빛은 서서히 꺼진다
    private void OnDied()
    {
        _player.Anim.SetFloat(DieSpeedId, 1f);
        _player.Anim.Play(DieState, 0, 0f);

        StartFade(0f, _fadeOutDuration);
    }

    // 등장: Die 애니메이션을 끝에서부터 역방향으로 재생 (게임 시작 때도 같다). 빛은 꺼진 상태에서 서서히 켜진다
    private void OnSpawned(Transform player, Vector3 delta)
    {
        if (player != transform) return;

        // 기본은 -1배속. 등장 시간이 클립보다 짧으면 그 시간 안에 끝나도록 더 빠르게 한다
        float speed = Mathf.Max(1f, _dieClipLength / Mathf.Max(0.01f, _player.SpawnDuration));
        _player.Anim.SetFloat(DieSpeedId, -speed);
        _player.Anim.Play(DieState, 0, 1f);

        if (_player.Light != null) _player.Light.intensity = 0f; // 꺼진 상태에서 시작
        StartFade(_baseIntensity, _player.SpawnDuration);
    }

    // 등장이 끝나 움직일 수 있게 되면 평소 애니메이션으로 되돌린다 (이후 전환은 각 State가 알아서 한다)
    private void OnSpawnFinished()
    {
        _player.Anim.SetFloat(DieSpeedId, 1f);
        _player.Anim.Play(IdleState);
    }

    // 빛 밝기를 target까지 duration 동안 서서히 바꾼다 (이전 연출이 남아 있으면 끊고 새로 시작)
    private void StartFade(float target, float duration)
    {
        if (_player.Light == null) return;

        StopFade();
        _routine = StartCoroutine(FadeRoutine(target, duration));
    }

    private void StopFade()
    {
        if (_routine == null) return;

        StopCoroutine(_routine);
        _routine = null;
    }

    private IEnumerator FadeRoutine(float target, float duration)
    {
        float start = _player.Light.intensity;

        if (duration <= 0f)
        {
            _player.Light.intensity = target;
            _routine = null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _player.Light.intensity = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        _player.Light.intensity = target;
        _routine = null;
    }
}
