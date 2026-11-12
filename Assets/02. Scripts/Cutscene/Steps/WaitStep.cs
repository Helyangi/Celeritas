using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 정해진 시간만큼 기다린다.
/// </summary>
[Serializable, CutsceneStepMenu("대기")]
public class WaitStep : CutsceneStep
{
    [SerializeField, Min(0f)] private float _seconds = 1f;

    public override string Summary => $"{_seconds}초";

    public override IEnumerator Execute(CutsceneManager manager)
    {
        if (_seconds > 0f) yield return new WaitForSeconds(_seconds);
    }
}
