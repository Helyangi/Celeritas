using UnityEngine;

/// <summary>
/// 대사를 말하는 인물 정보. 컷씬 대사와 NPC 대사가 함께 쓴다.
/// 효과음은 비워 둬도 되고, 리소스가 준비되면 이 에셋에서 켜기만 하면 대사가 나올 때 재생된다.
/// </summary>
[CreateAssetMenu(fileName = "Speaker", menuName = "Data/Dialogue/Speaker")]
public class SpeakerData : ScriptableObject
{
    public string DisplayName; // 대화창에 표시될 이름

    [Header("대사 효과음 (선택)")]
    public bool UseVoiceSFX;   // 이 인물의 대사가 나올 때 효과음을 낼지
    public SFXType VoiceSFX;   // 낼 효과음 종류 (SoundManager의 SFX 목록에 등록되어 있어야 한다)
}
