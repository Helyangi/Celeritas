using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 중 ESC(패드 Start)로 여는 일시정지 메뉴.
/// 시간은 GameTime의 일시정지 스위치로만 멈춘다 (히트스톱과 엉키지 않게).
/// 멈추기 전의 입력 상태를 기억했다가 그대로 돌려준다.
/// 컷신이나 대화 중이라 원래 꺼져 있던 조작이, 메뉴를 닫았다고 멋대로 켜지지 않게 하기 위해서다.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject _panel; // 켜고 끌 메뉴 전체
    [SerializeField] private string _mainMenuScene = "MainMenu";

    [Header("화면 표시")]
    [Tooltip("메인 메뉴처럼 카메라 위에 그려서 블룸(빛 번짐)을 받게 한다. 오버레이로 그리면 버튼 빛이 하얗게 뭉개진다.")]
    [SerializeField] private float _planeDistance = 1f;
    [SerializeField] private string _sortingLayer = "UI";
    [SerializeField] private int _sortingOrder = 500;

    private bool _open;
    private bool _playerInputWasEnabled;
    private bool _dialogueInputWasEnabled;

    public bool IsOpen => _open;

    private void Awake()
    {
        if (_panel != null) _panel.SetActive(false);
        EnsureEventSystem();
    }

    private void OnDestroy()
    {
        // 열린 채로 씬이 사라지면 시간이 멈춘 채 남지 않게 한다
        if (_open) GameTime.SetPaused(false);
    }

    private void Update()
    {
        if (!PausePressed()) return;

        if (_open) Resume();
        else Pause();
    }

    private static bool PausePressed()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) return true;

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null && gamepad.startButton.wasPressedThisFrame) return true;

        return false;
    }

    public void Pause()
    {
        if (_open) return;
        _open = true;

        GameTime.SetPaused(true);

        // 지금 상태를 기억해두고 끈다 (켜져 있던 것만 나중에 다시 켠다)
        InputManager input = InputManager.Instance;
        if (input != null)
        {
            _playerInputWasEnabled = input.IsPlayerInputEnabled;
            _dialogueInputWasEnabled = input.IsDialogueInputEnabled;
            input.DisablePlayerInput();
            input.DisableDialogueInput();
        }

        if (_panel != null)
        {
            AttachToCamera();
            _panel.SetActive(true);
        }
    }

    // 패널을 지금 카메라에 붙인다. 프리팹은 씬 카메라를 미리 참조할 수 없어서 열 때마다 찾는다.
    // 카메라가 없으면 오버레이 그대로 둔다 (메뉴가 안 보이는 것보다는 낫다).
    private void AttachToCamera()
    {
        Canvas canvas = _panel.GetComponent<Canvas>();
        Camera cam = Camera.main;
        if (canvas == null || cam == null) return;

        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = Mathf.Clamp(_planeDistance, cam.nearClipPlane + 0.01f, cam.farClipPlane - 0.01f);
        canvas.sortingLayerName = _sortingLayer;
        canvas.sortingOrder = _sortingOrder;
    }

    /// <summary>"계속하기" 버튼</summary>
    public void Resume()
    {
        if (!_open) return;
        _open = false;

        if (_panel != null) _panel.SetActive(false);

        InputManager input = InputManager.Instance;
        if (input != null)
        {
            if (_playerInputWasEnabled) input.EnablePlayerInput();
            if (_dialogueInputWasEnabled) input.EnableDialogueInput();
        }

        GameTime.SetPaused(false);
    }

    /// <summary>"메인으로" 버튼</summary>
    public void GoToMainMenu()
    {
        _open = false;

        // 컷씬이 돌고 있었다면 멈춘다. 컷씬 매니저는 씬을 넘어가도 유지돼서, 그냥 두면 메인 메뉴에서도 컷씬이 이어진다
        if (CutsceneManager.Instance != null) CutsceneManager.Instance.Abort();

        // 멈춘 채로 넘어가면 메인 메뉴가 시간 0으로 시작해서 페이드가 움직이지 않는다
        GameTime.SetPaused(false);
        SceneManager.LoadScene(_mainMenuScene);
    }

    // 버튼을 누르려면 EventSystem이 필요하다. 씬에 없으면 하나 만든다 (있는 씬에 또 만들면 경고가 난다)
    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        if (FindFirstObjectByType<EventSystem>() != null) return;

        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }
}
