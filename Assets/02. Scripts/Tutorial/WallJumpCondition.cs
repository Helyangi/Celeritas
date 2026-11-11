// 벽 점프를 하면 달성된다.
public class WallJumpCondition : TutorialCondition
{
    protected override void OnBegin()
    {
        Player.MovementStateChanged -= OnMovementStateChanged;
        Player.MovementStateChanged += OnMovementStateChanged;
    }

    protected override void OnEnd()
    {
        if (Player != null) Player.MovementStateChanged -= OnMovementStateChanged;
    }

    private void OnMovementStateChanged()
    {
        if (Player.IsWallJumping) IsMet = true;
    }
}
