using UnityEngine;

// 조준 버튼을 지정한 시간 이상 계속 누르고 있으면 달성된다.
public class AimHoldCondition : TutorialCondition
{
    [SerializeField] private float _holdSeconds = 0.5f;
    private float _held;

    protected override void OnBegin()
    {
        _held = 0f;
    }

    private void Update()
    {
        if (Player == null || IsMet) return;

        _held = Player.IsAiming ? _held + Time.deltaTime : 0f; // 버튼을 놓으면 처음부터 다시 센다
        if (_held >= _holdSeconds) IsMet = true;
    }
}
