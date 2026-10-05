using UnityEngine;

public class PlayerLightMoveState : PlayerBaseMovementState
{
    public PlayerLightMoveState(PlayerController controller) : base(controller) {}
    
    public override void EnterState()
    {
        controller.CanJump = true;
    }
    
    public override void FixedUpdateState()
    {
        LightMove();
    }
    
    private void LightMove()
    {
        Vector2 newPos = Vector2.MoveTowards
        (
            controller.RB.position,
            controller.Point,
            controller.Stats.LightMoveSpeed * Time.fixedDeltaTime
        );
        
        controller.RB.MovePosition(newPos);

        float distance = Vector2.Distance(controller.RB.position, controller.Point);
        if (distance <= 0.1f)
        {
            controller.ChangeMovementState(controller.FallState);
        }
    }
    
    public override void ExitState()
    {
        controller.RB.linearVelocityY = 0;
    }
}
