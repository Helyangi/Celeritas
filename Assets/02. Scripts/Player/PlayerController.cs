using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{   
    // 기본 변수
    public Rigidbody2D RB;
    public PlayerStats Stats;
    public GroundChecker GroundChecker;     // GroundChecker 연결
    public WallChecker WallChecker;         // WallChecker 연결
    public LightAimingLine LightAimingLine; // LightAimingLine과 연결
    
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
    public PlayerAimingState AimingState; // 아무것도 안하는 상태

    // 이름을 못 정했는데 마땅히 적을 곳을 생각 못해서 여기 적어봄
    [HideInInspector] public bool CanJump = true;
    [HideInInspector] public List<Vector2> LightPath = new List<Vector2>(); // 빛 이동 시 순서대로 거쳐갈 경로 (Aiming 상태가 채우고 LightMove 상태가 읽음)

    private void Awake()
    {
        if (RB == null)
        {
            RB = GetComponent<Rigidbody2D>();
        }
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

        // 기본 상태 설정
        ChangeMovementState(IdleState);
        ChangeActionState(AimingState);
    }

    private void Update()
    {
        // 입력 처리는 컨트롤러가 아니라 각 State 내부에서 직접 담당한다 (추후 InputManager 싱글톤 교체 대비)
        _currentMovementState?.UpdateState();
        _currentActionState?.UpdateState();
    }

    private void FixedUpdate()
    {
        // 물리 갱신이 필요한 상태 로직 처리
        _currentMovementState?.FixedUpdateState();
        _currentActionState?.FixedUpdateState();
    }

    // 현재 움직임 상태 변경 함수
    public void ChangeMovementState(PlayerBaseMovementState newState)
    {
        _currentMovementState?.ExitState();
        _currentMovementState = newState;
        _currentMovementState.EnterState();
    }

    // 현재 행동 상태 변경 함수
    public void ChangeActionState(PlayerBaseActionState newState)
    {
        _currentActionState?.ExitState();
        _currentActionState = newState;
        _currentActionState.EnterState();
    }
}
