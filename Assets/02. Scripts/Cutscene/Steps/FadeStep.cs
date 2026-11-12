using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 화면을 가림막으로 어둡게 덮거나(페이드 아웃), 덮인 화면을 밝게 걷어낸다(페이드 인).
/// </summary>
[Serializable, CutsceneStepMenu("화면 페이드")]
public class FadeStep : CutsceneStep
{
    public enum FadeType
    {
        [InspectorName("밝아지기 (페이드 인)")] FadeIn,
        [InspectorName("어두워지기 (페이드 아웃)")] FadeOut
    }

    [SerializeField] private FadeType _type = FadeType.FadeIn;
    [SerializeField, Min(0f)] private float _duration = 1f;
    [SerializeField] private Color _color = Color.black;

    public override string Summary => $"{(_type == FadeType.FadeIn ? "페이드 인" : "페이드 아웃")} {_duration}초";

    public override IEnumerator Execute(CutsceneManager manager)
    {
        ScreenFader fader = manager.Fader;
        if (fader == null)
        {
            Debug.LogWarning("[FadeStep] CutsceneManager에 ScreenFader가 연결되어 있지 않아 페이드를 건너뜁니다.");
            yield break;
        }

        fader.SetColor(_color);

        if (_type == FadeType.FadeIn)
        {
            fader.SetAlpha(1f); // 페이드 인은 항상 완전히 덮인 화면에서 시작한다
            yield return fader.FadeTo(0f, _duration);
        }
        else
        {
            yield return fader.FadeTo(1f, _duration);
        }
    }
}
