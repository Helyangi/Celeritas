using UnityEngine;

public class PlayerNoneState : PlayerBaseActionState
{
    public PlayerNoneState(PlayerController controller) : base(controller) {}

    public override void EnterState()
    {
        controller.LightAimingLine.SetAiming(false); // 조준 중이 아닐 때는 조준선을 숨긴다
    }
    public override void UpdateState()
    {
        // 버튼을 누르고 빛 이동 중이 아닐때 조준 시작
        if (GetAimHeld() && !controller.IsLightMoving)
        {
            controller.ChangeActionState(controller.AimingState);
        }
    }
}
