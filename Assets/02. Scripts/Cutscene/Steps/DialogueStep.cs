using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대사를 띄우고, 마지막 대사를 넘길 때까지 기다린다. (기존 DialogueManager의 대화창을 그대로 쓴다)
/// </summary>
[Serializable, CutsceneStepMenu("대사")]
public class DialogueStep : CutsceneStep
{
    [SerializeField] private List<DialogueLine> _lines = new List<DialogueLine>();

    public override string Summary => $"{_lines.Count}줄";

    public override IEnumerator Execute(CutsceneManager manager)
    {
        DialogueManager dialogue = DialogueManager.Instance;
        if (dialogue == null)
        {
            Debug.LogWarning("[DialogueStep] 씬에 DialogueManager가 없어서 대사를 건너뜁니다.");
            yield break;
        }

        bool ended = false;
        Action onEnded = () => ended = true;
        dialogue.DialogueEnded += onEnded;

        try
        {
            // 플레이어 조작은 CutsceneManager가 관리하므로 controlPlayer: false
            if (!dialogue.StartDialogue(_lines, "", false)) yield break;

            while (!ended) yield return null;
        }
        finally
        {
            dialogue.DialogueEnded -= onEnded;
        }
    }
}
