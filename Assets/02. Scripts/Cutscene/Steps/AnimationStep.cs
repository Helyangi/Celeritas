using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 대상의 Animator에서 상태 이름으로 애니메이션을 재생한다. (예: Idle, Run)
/// 컷씬 전용 동작이 필요하면 Animator에 상태를 추가하고 그 이름을 적으면 된다.
/// </summary>
[Serializable, CutsceneStepMenu("애니메이션 재생")]
public class AnimationStep : CutsceneStep
{
    public enum WaitMode
    {
        [InspectorName("기다리지 않음")] None,
        [InspectorName("한 번 끝날 때까지")] UntilFinished,
        [InspectorName("정한 시간만큼")] Seconds
    }

    [Tooltip("애니메이션을 재생할 대상 Id (플레이어는 Player). 대상이나 자식에 Animator가 있어야 한다")]
    [SerializeField] private string _actorId = CutsceneTargets.PlayerId;
    [Tooltip("Animator 안의 상태 이름")]
    [SerializeField] private string _stateName;
    [Tooltip("반복 애니메이션은 끝나지 않으니 '정한 시간만큼'을 쓴다")]
    [SerializeField] private WaitMode _wait = WaitMode.None;
    [SerializeField, Min(0f)] private float _seconds = 1f;

    public override string Summary => $"{_actorId}: {_stateName}";

    public override IEnumerator Execute(CutsceneManager manager)
    {
        Animator animator = CutsceneTargets.FindAnimator(_actorId);
        if (animator == null) yield break;
        if (!CutsceneTargets.PlayAnimation(animator, _stateName)) yield break;

        if (_wait == WaitMode.Seconds)
        {
            if (_seconds > 0f) yield return new WaitForSeconds(_seconds);
            yield break;
        }
        if (_wait != WaitMode.UntilFinished) yield break;

        yield return null; // 상태가 실제로 바뀔 때까지 한 프레임 기다린다
        if (animator == null) yield break;

        int hash = Animator.StringToHash(_stateName);
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (info.shortNameHash != hash) yield break; // 그새 다른 애니메이션으로 바뀌었다

        if (info.loop)
        {
            Debug.LogWarning($"[AnimationStep] '{_stateName}'은 반복 애니메이션이라 끝나지 않아요. '정한 시간만큼'을 써 주세요.");
            yield break;
        }

        float timeout = info.length * 2f + 1f;
        float time = 0f;
        while (animator != null)
        {
            info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.shortNameHash != hash || info.normalizedTime >= 1f) break;

            time += Time.deltaTime;
            if (time > timeout) break;
            yield return null;
        }
    }
}
