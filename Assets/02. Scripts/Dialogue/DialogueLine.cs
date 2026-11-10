using System;
using UnityEngine;

/// <summary>
/// 대사 한 줄. 줄마다 화자를 따로 정할 수 있어서 여러 인물이 주고받는 대화를 만들 수 있다.
/// </summary>
[Serializable]
public class DialogueLine
{
    public SpeakerData Speaker;          // 말하는 인물 (비우면 대화를 시작할 때 넘긴 기본 이름을 쓴다)
    [TextArea(2, 5)] public string Text; // 대사 내용

    public DialogueLine() {}

    public DialogueLine(SpeakerData speaker, string text)
    {
        Speaker = speaker;
        Text = text;
    }
}
