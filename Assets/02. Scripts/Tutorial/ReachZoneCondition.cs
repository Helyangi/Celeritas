using UnityEngine;

// 플레이어가 이 오브젝트의 BoxCollider2D(도달 지점)에 닿으면 달성된다. 도달 지점은 튜토리얼 박스 밖에 있어도 된다.
[RequireComponent(typeof(BoxCollider2D))]
public class ReachZoneCondition : TutorialCondition
{
    private BoxCollider2D _zone;
    private Collider2D[] _playerColliders;

    public override bool CountsOutsideZone => true; // 도달 지점이 튜토리얼 박스 밖일 수 있으므로 범위 밖에서도 판정한다

    private void Reset()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(4f, 4f);
    }

    private void Awake()
    {
        _zone = GetComponent<BoxCollider2D>();
    }

    protected override void OnBegin()
    {
        _playerColliders = Player.GetComponentsInChildren<Collider2D>();
    }

    private void Update()
    {
        if (Player == null || _zone == null || _playerColliders == null || IsMet) return;

        foreach (Collider2D col in _playerColliders)
        {
            if (col != null && col.enabled && _zone.Distance(col).isOverlapped) // 접촉 정보가 아니라 도형 겹침을 직접 계산한다
            {
                IsMet = true;
                return;
            }
        }
    }
}
