using System;
using System.Collections;

/// <summary>
/// 컷씬을 이루는 단계 하나의 공통 틀.
/// 새 연출이 필요하면 이 클래스를 상속한 단계를 하나 만들면 된다. (기존 단계와 매니저는 고칠 필요 없음)
/// - Execute: 단계가 할 일을 하고, 끝날 때까지 기다리는 코루틴
/// - 클래스에 [Serializable, CutsceneStepMenu("이름")]을 붙이면 CutsceneData 인스펙터의 "단계 추가" 메뉴에 나타난다
/// </summary>
[Serializable]
public abstract class CutsceneStep
{
    public abstract IEnumerator Execute(CutsceneManager manager);

    // 인스펙터 단계 제목 옆에 보일 한 줄 요약 (예: "3줄", "1.5초"). 필요한 단계만 재정의한다.
    public virtual string Summary => "";
}

/// <summary>
/// "단계 추가" 메뉴에 표시될 이름
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CutsceneStepMenuAttribute : Attribute
{
    public readonly string MenuName;

    public CutsceneStepMenuAttribute(string menuName)
    {
        MenuName = menuName;
    }
}
