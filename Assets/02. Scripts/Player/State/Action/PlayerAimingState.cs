using UnityEngine;

public class PlayerAimingState : PlayerBaseActionState
{
    public PlayerAimingState(PlayerController controller) : base(controller) {}
    
    private Vector2 _direction;
    private bool _canLightMove;
    
    public override void UpdateState()
    {
        RaycastHit2D hit = Physics2D.Raycast
        (
            controller.RB.position,
            _direction,
            controller.LightAimingLine.MaxDistance,
            controller.LightAimingLine.ContactLayer
        );

        HandleHitLayer(hit); // 닿은 레이어가 뭔지 판단
        DrawAimingLine(hit); // 랜더링

        if (_canLightMove && GetMouseInput())
        {
            controller.Point = hit.transform.position;
            controller.ChangeMovementState(controller.LightMoveState);
        }
    }
    private float GetDistance(RaycastHit2D hit) // 조준선 거리 구하는 함수
    {
        if (hit)
        {
            return hit.distance;
        }
        else
        {
            return controller.LightAimingLine.MaxDistance;
        }
    }
    
    private void DrawAimingLine(RaycastHit2D hit) // 조준선 거리 랜더링 해주는 함수
    {
        Vector2 myPos = controller.transform.position;

        _direction = (GetMousePos() - myPos).normalized;
        Vector2 endPos = myPos + _direction * GetDistance(hit);
        controller.LightAimingLine.DrawAimingLine(myPos, endPos);
    }
    
    private void HandleHitLayer(RaycastHit2D hit) // 닿은 레이어가 뭔지 판단하는 함수
    {
        if (hit.collider == null)
        {
            _canLightMove = false;
            controller.LightAimingLine.CoolingColor();
            return;
        }

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("LightOrb"))
        {
            _canLightMove = true;
            controller.LightAimingLine.HeatingColor();
        }
        else
        {
            _canLightMove = false;
            controller.LightAimingLine.CoolingColor();
        }
    }
}