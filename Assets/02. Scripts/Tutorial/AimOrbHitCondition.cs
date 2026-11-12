// 조준선이 빛 오브에 닿으면 달성된다. (이미 닿아 있는 상태로 시작해도 달성)
public class AimOrbHitCondition : TutorialCondition
{
    protected override void OnBegin()
    {
        IsMet = Player.IsAimingAtOrb;
        Player.AimingAtOrbChanged -= OnAimingAtOrbChanged;
        Player.AimingAtOrbChanged += OnAimingAtOrbChanged;
    }

    protected override void OnEnd()
    {
        if (Player != null) Player.AimingAtOrbChanged -= OnAimingAtOrbChanged;
    }

    private void OnAimingAtOrbChanged(bool hit)
    {
        if (hit) IsMet = true;
    }
}
