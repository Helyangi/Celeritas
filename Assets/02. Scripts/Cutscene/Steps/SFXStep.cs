using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 효과음을 한 번 재생한다. (기다리지 않고 바로 다음 단계로 넘어간다)
/// 새 효과음은 SFXType에 종류를 추가하고, SFXData 에셋을 만들어 SoundManager 목록에 등록하면 여기서 고를 수 있다.
/// </summary>
[Serializable, CutsceneStepMenu("효과음")]
public class SFXStep : CutsceneStep
{
    [SerializeField] private SFXType _sfx;
    [Tooltip("소리가 날 위치의 CutsceneActor Id. 비워 두면 카메라 위치에서 재생한다.")]
    [SerializeField] private string _positionActorId;

    public override string Summary => _sfx.ToString();

    public override IEnumerator Execute(CutsceneManager manager)
    {
        if (SoundManager.Instance == null)
        {
            Debug.LogWarning("[SFXStep] 씬에 SoundManager가 없어서 효과음을 건너뜁니다.");
            yield break;
        }

        Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        if (!string.IsNullOrEmpty(_positionActorId) && CutsceneActor.TryFind(_positionActorId, out CutsceneActor actor))
        {
            position = actor.transform.position;
        }

        SoundManager.Instance.PlaySFX(_sfx, position);
    }
}
