using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {get; private set;}
    
    [SerializeField] private float _bufferTime = 0.1f; // 버퍼 타임
    
    [HideInInspector] public Vector2 AimingPos; // 조준 위치
    [HideInInspector] public float MoveDir; // 좌우 이동 방향
    [HideInInspector] public bool IsJump; // 점프 클릭 여부
    [HideInInspector] public bool IsPressedNextDialogue; // 다음 대화 입력 여부
    [HideInInspector] public bool IsInteract; // 상호작용 여부
    
    private float _jumpTimer; // 점프 타이머

    // 컷씬처럼 스크립트가 대신 넣는 이동 입력 (null이면 키보드 입력을 그대로 쓴다)
    private float? _scriptedMoveDir;
    
    private InputSystem_Actions _actionMaps;
    
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

        _actionMaps = new InputSystem_Actions();

        InitialSubcription(); // 초기 구독
        EnablePlayerInput();
    }
    
    private void Update()
    {
        // 버퍼 타임 체크
        BufferTimeCheck(ref IsJump, ref _jumpTimer);

        // 조준 위치 입력
        Vector2 mousePos = _actionMaps.Player.AimingPos.ReadValue<Vector2>();
        AimingPos = Camera.main.ScreenToWorldPoint(mousePos);

        // 좌우 이동 입력 (스크립트가 넣은 입력이 있으면 그걸 우선한다)
        MoveDir = _scriptedMoveDir ?? _actionMaps.Player.Move.ReadValue<Vector2>().x;
        
        // 다음 대화 입력
        IsPressedNextDialogue = _actionMaps.Dialogue.Next.WasPressedThisFrame();
        
        // 상호작용 입력
        IsInteract = _actionMaps.Player.Interact.WasPressedThisFrame();
    }
    public bool IsAimHeld => _actionMaps.Player.LightEmission.IsPressed();

    // 플레이어 인풋 활성 & 비활성
    public void EnablePlayerInput()
    {
        _actionMaps.Player.Enable();
    }
    
    public void DisablePlayerInput()
    {
        _actionMaps.Player.Disable();
    }

    // 대화 인풋 활성 & 비활성
    public void EnableDialogueInput()
    {
        _actionMaps.Dialogue.Enable();
    }

    public void DisableDialogueInput()
    {
        _actionMaps.Dialogue.Disable();
    }

    // 스크립트 입력 (컷씬에서 플레이어가 스스로 움직일 때 사용)
    // 키보드 입력이 꺼져 있어도 동작한다. 다 쓰면 ClearScriptedInput으로 꼭 비워야 한다.
    public void SetScriptedMove(float direction)
    {
        _scriptedMoveDir = Mathf.Clamp(direction, -1f, 1f);
        MoveDir = _scriptedMoveDir.Value; // 이번 프레임부터 바로 반영
    }

    public void ScriptedJump()
    {
        _jumpTimer = _bufferTime;
        IsJump = true;
    }

    public void ClearScriptedInput()
    {
        _scriptedMoveDir = null;
        MoveDir = 0f;
    }
    
    // 구독
    private void InitialSubcription()
    {
        _actionMaps.Player.Jump.performed += OnJump; // 점프 구독
    }

    // 대화 시작
    public void EnterDialogueMode()
    {
        DisablePlayerInput();
        EnableDialogueInput();
    }

    // 대화 끝
    public void ExitDialogueMode()
    {
        DisableDialogueInput();
        EnablePlayerInput();
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        _jumpTimer = _bufferTime;
        IsJump = true;
    }
    
    private void BufferTimeCheck(ref bool input, ref float timer)
    {
        if (timer <= 0)
        {
            input = false;
        }
        else
        {
            timer -= Time.deltaTime;
            
        }
    }
}
