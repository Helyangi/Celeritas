// 벽에 붙으면 달성된다.
public class WallGrabCondition : TutorialCondition
{
    protected override void OnBegin()
    {
        IsMet = Player.IsWallGrabbing;
        Player.MovementStateChanged -= OnMovementStateChanged;
        Player.MovementStateChanged += OnMovementStateChanged;
    }

    protected override void OnEnd()
    {
        if (Player != null) Player.MovementStateChanged -= OnMovementStateChanged;
    }

    private void OnMovementStateChanged()
    {
        if (Player.IsWallGrabbing) IsMet = true;
    }
}
