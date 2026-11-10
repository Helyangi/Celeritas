using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 컷씬 하나의 대본. 위에서부터 순서대로 실행될 단계 목록을 담는다.
/// 새 컷씬은 Create > Data > Cutscene > Cutscene 으로 에셋을 만들고, 인스펙터의 "+ 단계 추가"로 단계를 쌓으면 된다.
/// </summary>
[CreateAssetMenu(fileName = "Cutscene", menuName = "Data/Cutscene/Cutscene")]
public class CutsceneData : ScriptableObject
{
    [Tooltip("컷씬을 구분하는 고유 이름. 비워 두면 에셋 이름을 쓴다.")]
    [SerializeField] private string _id;

    [SerializeReference] private List<CutsceneStep> _steps = new List<CutsceneStep>();

    public string Id => string.IsNullOrEmpty(_id) ? name : _id;
    public IReadOnlyList<CutsceneStep> Steps => _steps;
}
