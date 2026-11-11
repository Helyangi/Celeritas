using UnityEngine;

// 점프 가능 상태를 파티클로 보여주는 연출 컴포넌트.
// 반딧불: 낙하 중이면서 점프 기회가 있을 때만 떠다닌다 (공중에서 쓸 수 있는 한 번의 점프를 알려준다)
// 점프 소모: 낙하 중에 쓰면 강하게, 그 외(지상·벽점프)에는 약하게 터진다
// 점프 획득: 짧게 모여드는 연출 (항상)
// PlayerController의 이벤트만 구독하며, PlayerController는 이 클래스를 알지 못한다.
[RequireComponent(typeof(PlayerController))]
public class PlayerJumpEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _fireflies; // 낙하 중 점프 기회가 있는 동안 떠다니는 반딧불 (Loop)
    [SerializeField] private ParticleSystem _loseBurst; // 점프를 쓰면 흩어지는 파티클
    [SerializeField] private ParticleSystem _gainBurst; // 점프 기회를 얻으면 짧게 모여드는 파티클

    [Header("점프 소모 - 낙하 중 (강)")]
    [SerializeField] private int _strongCount = 24;
    [SerializeField] private float _strongSpeedScale = 1.4f;
    [SerializeField] private float _strongSizeScale = 1.3f;

    [Header("점프 소모 - 그 외 (약)")]
    [SerializeField] private int _weakCount = 6;
    [SerializeField] private float _weakSpeedScale = 0.6f;
    [SerializeField] private float _weakSizeScale = 0.8f;

    [Header("점프 획득")]
    [SerializeField] private int _gainCount = 6;
    [SerializeField] private float _gainCooldown = 0.1f; // 연속으로 얻어도 이 시간 안에는 획득 연출을 겹쳐 재생하지 않는다

    private PlayerController _player;
    private bool _suppressed; // 사망·등장 연출 중에는 연출을 끈다
    private float _lastGainTime = float.NegativeInfinity;

    // 소모 연출의 기본 속도/크기 (강약은 이 값에 배율을 곱해 만든다)
    private Vector2 _baseSpeed;
    private Vector2 _baseSize;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();

        if (_loseBurst != null)
        {
            var main = _loseBurst.main;
            _baseSpeed = new Vector2(main.startSpeed.constantMin, main.startSpeed.constantMax);
            _baseSize = new Vector2(main.startSize.constantMin, main.startSize.constantMax);
        }
    }

    private void OnEnable()
    {
        _player.JumpAvailabilityChanged += OnJumpAvailabilityChanged;
        _player.MovementStateChanged += RefreshFireflies;
        _player.Died += OnDied;
        _player.SpawnFinished += OnSpawnFinished;
        PlayerController.Spawned += OnSpawned;
    }

    private void OnDisable()
    {
        _player.JumpAvailabilityChanged -= OnJumpAvailabilityChanged;
        _player.MovementStateChanged -= RefreshFireflies;
        _player.Died -= OnDied;
        _player.SpawnFinished -= OnSpawnFinished;
        PlayerController.Spawned -= OnSpawned;
    }

    private void Start()
    {
        // 등장 연출 도중에 시작될 수도 있으니 현재 상태를 보고 시작한다
        _suppressed = _player.IsRespawning;
        RefreshFireflies();
    }

    private void OnJumpAvailabilityChanged(bool canJump)
    {
        if (_suppressed) return;

        RefreshFireflies();

        if (canJump)
        {
            if (Time.time - _lastGainTime >= _gainCooldown)
            {
                _lastGainTime = Time.time;
                if (_gainBurst != null) _gainBurst.Emit(_gainCount);
            }
        }
        else
        {
            // 점프 상태로 진입하는 시점이라 직전 상태가 낙하였는지로 세기를 정한다
            bool fromFall = _player.PreviousMovementState == _player.FallState;
            EmitLose(fromFall);
        }
    }

    private void OnDied()
    {
        _suppressed = true;
        SetFireflies(false);
    }

    private void OnSpawned(Transform player, Vector3 delta)
    {
        if (player != transform) return;
        _suppressed = true;
        SetFireflies(false);
    }

    private void OnSpawnFinished()
    {
        _suppressed = false;
        RefreshFireflies();
    }

    // 반딧불은 낙하 중이고 점프 기회가 있을 때만 보인다
    private void RefreshFireflies()
    {
        SetFireflies(!_suppressed && _player.CanJump && _player.IsFalling);
    }

    private void SetFireflies(bool on)
    {
        if (_fireflies == null) return;

        if (on)
        {
            if (!_fireflies.isPlaying) _fireflies.Play();
        }
        else if (_fireflies.isPlaying || _fireflies.particleCount > 0)
        {
            _fireflies.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void EmitLose(bool strong)
    {
        if (_loseBurst == null) return;

        float speedScale = strong ? _strongSpeedScale : _weakSpeedScale;
        float sizeScale = strong ? _strongSizeScale : _weakSizeScale;

        // 이미 떠 있는 파티클에는 영향이 없고, 지금부터 만들어지는 파티클에만 적용된다
        var main = _loseBurst.main;
        main.startSpeed = new ParticleSystem.MinMaxCurve(_baseSpeed.x * speedScale, _baseSpeed.y * speedScale);
        main.startSize = new ParticleSystem.MinMaxCurve(_baseSize.x * sizeScale, _baseSize.y * sizeScale);

        _loseBurst.Emit(strong ? _strongCount : _weakCount);
    }
}
