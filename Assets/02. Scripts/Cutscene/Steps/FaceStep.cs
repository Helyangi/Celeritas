using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 대상이 왼쪽/오른쪽, 또는 다른 대상 쪽을 바라보게 한다. (바로 다음 단계로 넘어간다)
/// </summary>
[Serializable, CutsceneStepMenu("바라보기")]
public class FaceStep : CutsceneStep
{
    public enum FaceMode
    {
        [InspectorName("오른쪽")] Right,
        [InspectorName("왼쪽")] Left,
        [InspectorName("대상 쪽")] Target
    }

    [Tooltip("방향을 바꿀 대상 Id (플레이어는 Player)")]
    [SerializeField] private string _actorId = CutsceneTargets.PlayerId;
    [SerializeField] private FaceMode _mode = FaceMode.Right;
    [Tooltip("'대상 쪽'일 때 바라볼 이름표 Id")]
    [SerializeField] private string _lookAtId;

    public override string Summary
    {
        get
        {
            string where = _mode == FaceMode.Right ? "오른쪽" : _mode == FaceMode.Left ? "왼쪽" : $"{_lookAtId} 쪽";
            return $"{_actorId} → {where}";
        }
    }

    public override IEnumerator Execute(CutsceneManager manager)
    {
        bool faceRight;

        if (_mode == FaceMode.Target)
        {
            Transform self = CutsceneTargets.FindTransform(_actorId);
            Transform other = CutsceneTargets.FindTransform(_lookAtId);
            if (self == null || other == null) yield break;

            float distance = other.position.x - self.position.x;
            if (Mathf.Abs(distance) < 0.001f) yield break; // 같은 자리면 그대로 둔다

            faceRight = distance > 0f;
        }
        else
        {
            faceRight = _mode == FaceMode.Right;
        }

        CutsceneTargets.Face(_actorId, faceRight);
    }
}
