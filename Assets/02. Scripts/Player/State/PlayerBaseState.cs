using UnityEngine;
using UnityEngine.InputSystem;

public abstract class PlayerBaseState
{
    protected PlayerController controller;
    public PlayerBaseState(PlayerController controller)
    {
        this.controller = controller;
    }
    
    public virtual void EnterState() {}
    public virtual void UpdateState() {}
    public virtual void FixedUpdateState() {}
    // public virtual void OnCollisionEnter2DState(Collision2D other) {}
    public virtual void ExitState() {}

    // 이동 입력값을 읽어오는 함수. 각 State가 컨트롤러를 거치지 않고 이 함수를 통해 직접 입력을 받아온다.
    // TODO: MVP 이후 InputManager(싱글톤) 도입 시 이 부분만 교체하면 됨 (예: return InputManager.Instance.MoveInput;)
    protected bool GetMouseInput()
    {
        return Input.GetMouseButtonDown(0);
    }
    
    protected Vector2 GetMousePos()
    {
        Vector2 mouseScreenPos = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }
    
    protected float GetMoveInput()
    {
        return Input.GetAxisRaw("Horizontal");
    }

    protected bool GetJumpInput()
    {
        return Input.GetKeyDown(KeyCode.Space);
    }
}