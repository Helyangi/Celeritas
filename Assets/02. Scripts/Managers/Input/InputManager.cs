using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance {get; private set;}
    [SerializeField] private float _bufferTime = 0.1f; // 버퍼 타임
    [HideInInspector] public Vector2 AimingPos; // 조준 위치
    [HideInInspector] public float MoveDir; // 좌우 이동 방향
    [HideInInspector] public bool IsJump; // 점프 클릭 여부
    private float _jumpTimer; // 점프 타이머
    [HideInInspector] public bool IsLightEmission; // 빛 방출 여부
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
    }
    private void LateUpdate()
    {
        IsLightEmission = false;
    }
    public void EnablePlayerInput()
    {
        _actionMaps.Player.Enable();
    }
    public void DisablePlayerInput()
    {
        _actionMaps.Player.Disable();
    }
    private void InitialSubcription()
    {
        _actionMaps.Player.Jump.performed += OnJump; // 점프 구독
        _actionMaps.Player.LightEmission.performed += OnLightEmission; // 빛 방출 구독
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        _jumpTimer = _bufferTime;
        IsJump = true;
    }
    private void OnLightEmission(InputAction.CallbackContext context)
    {
        IsLightEmission = true;
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