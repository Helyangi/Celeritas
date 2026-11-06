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

        // 좌우 이동 입력
        MoveDir = _actionMaps.Player.Move.ReadValue<Vector2>().x;
        
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