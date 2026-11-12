using UnityEngine;

// 좌우 이동 키(A/D, 방향키)로 지정한 시간 이상 이동하면 달성된다.
public class MoveInputCondition : TutorialCondition
{
    [SerializeField] private float _moveSeconds = 0.3f;
    private float _moved;

    protected override void OnBegin()
    {
        _moved = 0f;
    }

    private void Update()
    {
        if (Player == null || IsMet) return;

        _moved = Player.IsMoving ? _moved + Time.deltaTime : 0f; // 멈추면 처음부터 다시 센다
        if (_moved >= _moveSeconds) IsMet = true;
    }
}
