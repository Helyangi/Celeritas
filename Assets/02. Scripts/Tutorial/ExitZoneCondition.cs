using UnityEngine;

// 플레이어가 지정한 구역(보통 이 튜토리얼의 트리거 박스)에 들어왔다가 벗어나면 달성된다.
public class ExitZoneCondition : TutorialCondition
{
    [SerializeField] private Collider2D _zone;

    private Collider2D[] _playerColliders;
    private bool _wasInside; // 구역 안에 있었던 적이 있어야 벗어난 것으로 인정한다

    public override bool CountsOutsideZone => true; // 벗어나는 것이 조건이므로 범위 밖에서도 판정한다

    protected override void OnBegin()
    {
        _playerColliders = Player.GetComponentsInChildren<Collider2D>();
        _wasInside = false;
    }

    private void Update()
    {
        if (Player == null || _zone == null || IsMet) return;

        if (IsInside()) _wasInside = true;
        else if (_wasInside) IsMet = true;
    }

    private bool IsInside()
    {
        foreach (Collider2D col in _playerColliders)
        {
            if (col != null && col.enabled && _zone.Distance(col).isOverlapped) return true; // 접촉 정보가 아니라 도형 겹침을 직접 계산한다
        }
        return false;
    }
}
