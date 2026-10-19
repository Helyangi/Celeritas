using System.Collections;
using UnityEngine;

public class PlayerWallJumpState : PlayerBaseMovementState
{
    public PlayerWallJumpState(PlayerController controller) : base(controller) {}

    private Coroutine _wallJumpCoroutine;

    public override void EnterState()
    {
        _wallJumpCoroutine = controller.StartCoroutine(WallJumpCoroutine());
    }

    private IEnumerator WallJumpCoroutine()
    {
        controller.RB.linearVelocity = Vector2.zero;
        
        Jump(new Vector2(controller.WallChecker.GetWallDirection() * -1, 1));
        
        yield return new WaitForSeconds(0.2f);

        controller.RB.linearVelocityX = 0f;
        controller.ChangeMovementState(controller.FallState);
    }

    public override void ExitState()
    {
        if (_wallJumpCoroutine != null)
        {
            controller.StopCoroutine(_wallJumpCoroutine);
            _wallJumpCoroutine = null;
        }
    }
}
