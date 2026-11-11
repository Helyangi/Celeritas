// 점프하지 않고 떨어지는 중(낙하)에 공중에서 점프하면 달성된다.
// 낙하 상태에서 점프 상태로 넘어간 경우만 인정하므로, 바닥에서 한 점프나 벽 점프는 해당되지 않는다.
public class AirJumpCondition : TutorialCondition
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
        if (Player.IsJumping && Player.PreviousMovementState == Player.FallState) IsMet = true;
    }
}
