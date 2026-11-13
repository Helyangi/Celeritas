using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{   
    // 기본 변수
    public Rigidbody2D RB;
    public BoxCollider2D BoxCol;
    public Animator Anim;
    public SpriteRenderer Renderer;
    public PlayerStats Stats;
    public GroundChecker GroundChecker;     // GroundChecker 연결
    public WallChecker WallChecker;         // WallChecker 연결
    public LightAimingLine LightAimingLine; // LightAimingLine과 연결
    public Light2D Light; // 광원
    
    // 현재 상태 담당
    private PlayerBaseMovementState _currentMovementState; // 현재 움직임 상태 담당
    private PlayerBaseActionState _currentActionState;     // 현재 행동 상태 담당

    // 움직임 상태 정의
    public PlayerIdleState IdleState; // 아무 움직임도 없는 상태
    public PlayerMoveState MoveState; // 이동 중인 상태
    public PlayerJumpState JumpState; // 점프 상태
    public PlayerFallState FallState; // 낙하 상태
    public PlayerLightMoveState LightMoveState; // 빛 이동 상태
    public PlayerWallGrabState WallGrabState;   // 벽잡기 상태
    public PlayerWallJumpState WallJumpState;   // 벽점프 상태

    // 행동 상태 정의
    public PlayerAimingState AimingState; // 조준 상태
    public PlayerNoneState NoneState; // 아무것도 안하는 상태

    // 플레이어 플래그 변수
    private bool _canJump = true;
    public bool CanJump // 값이 실제로 바뀔 때만 JumpAvailabilityChanged가 발생한다
    {
        get => _canJump;
        set
        {
            if (_canJump == value) return;
            _canJump = value;
            JumpAvailabilityChanged?.Invoke(value);
        }
    }
    public bool IsLightMoving => _currentMovementState  == LightMoveState;
    public bool IsFalling => _currentMovementState == FallState;
    public bool IsMoving => _currentMovementState == MoveState;
    public bool IsWallGrabbing => _currentMovementState == WallGrabState;
    public bool IsWallJumping => _currentMovementState == WallJumpState;
    public bool IsJumping => _currentMovementState == JumpState || _currentMovementState == WallJumpState;
    public bool IsAiming => _currentActionState == AimingState;
    public bool IsAimingAtOrb { get; private set; } // 지금 조준선이 빛 오브에 닿아 있는가 (조준 상태가 매 프레임 갱신)
    public PlayerBaseMovementState PreviousMovementState { get; private set; } // 직전 움직임 상태 (상태 진입 시점에 "어디서 왔는지" 알 수 있다)

    // 리스폰 & 체크포인트 관련 변수
    private Vector3 _checkpoint;
    private bool _isRespawning;

    // 등장(게임 시작/리스폰) 연출 시간. 이 시간 동안은 물리와 입력이 멈춘 채로 나타난다.
    [SerializeField] private float _spawnDuration = 0.6f;
    public Vector2 InitialVector;
    public float SpawnDuration => _spawnDuration;
    public bool IsRespawning => _isRespawning;

    // 연출/카메라 등이 구독하는 이벤트. 이 클래스는 구독자를 알지 못한다.
    public static event System.Action<Transform, Vector3> Spawned; // (플레이어, 이동한 거리) 게임 시작·리스폰 때 위치가 정해진 직후
    public event System.Action Died;                                // 죽는 순간
    public event System.Action SpawnFinished;                       // 등장 연출이 끝나고 조작이 가능해진 순간
    public event System.Action ActionStateChanged;                  // 행동 상태(조준 등)가 바뀐 직후
    public event System.Action<bool> AimingAtOrbChanged;            // 조준선이 빛 오브에 닿기 시작/떨어진 순간
    public event System.Action MovementStateChanged;                // 움직임 상태가 바뀐 직후
    public event System.Action<bool> JumpAvailabilityChanged;       // 점프 가능 여부가 바뀐 순간 (true: 점프 기회 획득, false: 소모)
    public event System.Action<Vector2, Vector2> LightShot;         // 빛 이동을 시작한 순간 (발사 위치, 진행 방향)
    public event System.Action<Vector2, Vector2> LightReflected;    // 거울에서 꺾인 순간 (꺾인 위치, 꺾인 뒤 진행 방향)

    // 빛 이동 상태가 부르는 알림들 (연출 컴포넌트가 구독한다)
    public void RaiseLightShot(Vector2 position, Vector2 direction) => LightShot?.Invoke(position, direction);
    public void RaiseLightReflected(Vector2 position, Vector2 direction) => LightReflected?.Invoke(position, direction);

    private void Awake()
    {
        InitialVector = BoxCol.size;

        if (RB == null)
        {
            RB = GetComponent<Rigidbody2D>();
        }
        
        // 초기 체크포인트 설정
        _checkpoint = transform.position;
    }

    // 상태 정의
    private void Start()
    {
        // 움직임 상태 변수 초기화
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
        JumpState = new PlayerJumpState(this);
        FallState = new PlayerFallState(this);
        WallGrabState = new PlayerWallGrabState(this);
        WallJumpState = new PlayerWallJumpState(this);
        LightMoveState = new PlayerLightMoveState(this);

        // 행동 상태 변수 초기화
        AimingState = new PlayerAimingState(this);
        NoneState = new PlayerNoneState(this);

        // 기본 상태 설정
        ChangeMovementState(IdleState);
        ChangeActionState(NoneState); // 평소에는 조준하지 않고, 버튼을 눌러야 조준이 시작된다
    }

    private void Update()
    {
        // 리스폰 중 입력 감지 X
        if (_isRespawning) return;
        
        // 입력 처리는 컨트롤러가 아니라 각 State 내부에서 직접 담당한다 (추후 InputManager 싱글톤 교체 대비)
        _currentMovementState?.UpdateState();
        _currentActionState?.UpdateState();
    }

    private void FixedUpdate()
    {
        // 리스폰 중 입력 감지 X
        if (_isRespawning) return;
        
        // 물리 갱신이 필요한 상태 로직 처리
        _currentMovementState?.FixedUpdateState();
        _currentActionState?.FixedUpdateState();
    }

    // 현재 움직임 상태 변경 함수
    public void ChangeMovementState(PlayerBaseMovementState newState)
    {
        _currentMovementState?.ExitState();
        PreviousMovementState = _currentMovementState;
        _currentMovementState = newState;
        _currentMovementState.EnterState();
        MovementStateChanged?.Invoke();
    }

    // 현재 행동 상태 변경 함수
    public void ChangeActionState(PlayerBaseActionState newState)
    {
        _currentActionState?.ExitState();
        _currentActionState = newState;
        _currentActionState.EnterState();
        ActionStateChanged?.Invoke();
    }

    // 조준선이 빛 오브에 닿아 있는지 알려주는 함수 (조준 상태가 호출). 값이 바뀔 때만 이벤트가 발생한다
    public void SetAimingAtOrb(bool value)
    {
        if (IsAimingAtOrb == value) return;
        IsAimingAtOrb = value;
        AimingAtOrbChanged?.Invoke(value);
    }

    // 체크포인트 갱신 함수
    public void SetCheckpoint(Vector3 newCheckpoint)
    {
        if (_isRespawning) return;
        _checkpoint = newCheckpoint;
    }

    /// <summary>
    /// 게임 시작·저장 복원 때 플레이어를 이 위치에 등장시킨다. (리스폰도 같은 경로를 쓴다)
    /// </summary>
    public void Spawn(Vector3 position)
    {
        if (_isRespawning) return;
        StartCoroutine(SpawnRoutine(position));
    }
    
    // 리스폰 함수
    public void Respawn()
    {
        if (_isRespawning) return;
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        _isRespawning = true;
        RB.simulated = false;

        InputManager.Instance.DisablePlayerInput();
        ChangeMovementState(IdleState);
        ChangeActionState(NoneState);
        Died?.Invoke();
        
        yield return new WaitForSeconds(Stats.RespawnTime);

        // 행동 상태는 위에서 이미 NoneState가 되었고, 조준은 버튼을 다시 눌러야 시작된다
        SoundManager.Instance.PlaySFX(SFXType.Respawn, transform.position);

        yield return StartCoroutine(SpawnRoutine(_checkpoint));

        InputManager.Instance.EnablePlayerInput();
    }

    // 위치를 정하고 등장 연출 시간만큼 기다린 뒤 조작을 풀어주는 공통 루틴 (게임 시작 / 리스폰)
    private IEnumerator SpawnRoutine(Vector3 position)
    {
        _isRespawning = true;
        RB.simulated = false;

        Vector3 delta = position - transform.position;
        transform.position = position;
        RB.linearVelocity = Vector2.zero;

        if (LightAimingLine != null && LightAimingLine.LightPath != null) LightAimingLine.LightPath.Clear();
        CanJump = true;

        Spawned?.Invoke(transform, delta);

        yield return new WaitForSeconds(_spawnDuration);

        RB.simulated = true;
        _isRespawning = false;
        SpawnFinished?.Invoke();
    }
}
