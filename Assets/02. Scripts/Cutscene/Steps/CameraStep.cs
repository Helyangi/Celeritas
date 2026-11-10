using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 컷씬 카메라로 전환한다. Id를 비워 두면 컷씬 카메라를 해제하고 평소 카메라로 돌아간다.
/// 전환은 CinemachineBrain의 블렌드 설정대로 부드럽게 이어진다.
/// </summary>
[Serializable, CutsceneStepMenu("카메라 전환")]
public class CameraStep : CutsceneStep
{
    [Tooltip("CinemachineCamera가 붙은 CutsceneActor의 Id. 비워 두면 평소 카메라로 돌아간다.")]
    [SerializeField] private string _cameraActorId;
    [Tooltip("카메라 이동(블렌드)이 끝날 때까지 기다린 뒤 다음 단계로 넘어간다")]
    [SerializeField] private bool _waitForBlend = true;

    public override string Summary => string.IsNullOrEmpty(_cameraActorId) ? "평소 카메라로" : _cameraActorId;

    public override IEnumerator Execute(CutsceneManager manager)
    {
        if (string.IsNullOrEmpty(_cameraActorId))
        {
            manager.ReleaseCamera();
        }
        else
        {
            CinemachineCamera camera = CutsceneActor.FindComponent<CinemachineCamera>(_cameraActorId);
            if (camera == null) yield break;

            manager.FocusCamera(camera);
        }

        if (!_waitForBlend) yield break;

        CinemachineBrain brain = UnityEngine.Object.FindFirstObjectByType<CinemachineBrain>();
        if (brain == null) yield break;

        // 우선순위가 바뀐 뒤 브레인이 블렌드를 시작할 때까지 잠깐 기다린다
        yield return null;
        yield return null;

        while (brain != null && brain.IsBlending) yield return null;
    }
}
