using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 다른 씬으로 이동한다. CutsceneManager는 씬을 넘어가도 유지되므로, 이 단계 뒤에 단계를 더 두면
/// 새 씬에서 이어서 실행된다. (예: 페이드 아웃 → 씬 이동 → 페이드 인)
/// </summary>
[Serializable, CutsceneStepMenu("씬 이동")]
public class LoadSceneStep : CutsceneStep
{
    [Tooltip("이동할 씬 이름 (Build Settings의 씬 목록에 있어야 한다)")]
    [SerializeField] private string _sceneName;

    public override string Summary => _sceneName;

    public override IEnumerator Execute(CutsceneManager manager)
    {
        if (string.IsNullOrEmpty(_sceneName))
        {
            Debug.LogWarning("[LoadSceneStep] 이동할 씬 이름이 비어 있습니다.");
            yield break;
        }

        if (!Application.CanStreamedLevelBeLoaded(_sceneName))
        {
            Debug.LogError($"[LoadSceneStep] '{_sceneName}' 씬을 불러올 수 없습니다. Build Settings의 씬 목록에 있는지 확인하세요.");
            yield break;
        }

        manager.ReleaseCamera(); // 지금 씬의 컷씬 카메라는 씬과 함께 사라지므로 미리 정리한다

        AsyncOperation load = SceneManager.LoadSceneAsync(_sceneName);
        while (!load.isDone) yield return null;

        // 씬이 바뀔 때 다른 매니저가 플레이어 입력을 다시 켜므로, 컷씬이 끝날 때까지 다시 막는다
        manager.LockPlayerInput();
    }
}
