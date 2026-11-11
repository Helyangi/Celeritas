// 빛 이동이 시작되면 달성된다.
public class LightMoveCondition : TutorialCondition
{
    protected override void OnBegin()
    {
        IsMet = Player.IsLightMoving;
        Player.MovementStateChanged -= OnMovementStateChanged;
        Player.MovementStateChanged += OnMovementStateChanged;
    }

    protected override void OnEnd()
    {
        if (Player != null) Player.MovementStateChanged -= OnMovementStateChanged;
    }

    private void OnMovementStateChanged()
    {
        if (Player.IsLightMoving) IsMet = true;
    }
}
