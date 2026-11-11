using UnityEngine;

// 튜토리얼 단계의 완료 조건. 새 조건은 이 클래스를 상속해서 만든다.
// 시퀀스가 단계를 시작할 때 Begin, 끝날 때 End를 부른다. 한 번 달성되면(IsMet) 단계가 끝날 때까지 그대로 유지(래치)된다.
public abstract class TutorialCondition : MonoBehaviour
{
    public bool IsMet { get; protected set; }
    protected PlayerController Player { get; private set; }

    // 기본적으로 조건은 플레이어가 튜토리얼 범위 안에 있을 때만 판정한다.
    // 범위를 벗어나는 것 자체가 조건인 경우처럼 범위 밖에서도 판정해야 하면 true로 바꾼다.
    public virtual bool CountsOutsideZone => false;

    public void Begin(PlayerController player)
    {
        Player = player;
        IsMet = false;
        enabled = true; // Update가 필요한 조건은 활성화된 동안만 돈다
        OnBegin();
    }

    public void End()
    {
        OnEnd();
        enabled = false;
    }

    protected abstract void OnBegin();
    protected virtual void OnEnd() { }
}
