using UnityEngine;

public class PlayerNoneState : PlayerBaseActionState
{
    public PlayerNoneState(PlayerController controller) : base(controller) {}

    public override void EnterState()
    {
        controller.LightAimingLine.SetAiming(false);
    }
    public override void ExitState()
    {
        controller.LightAimingLine.SetAiming(true);
    }
}
