using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 스테이지 씬마다 하나씩 두는 컴포넌트.
/// 씬이 시작될 때 저장된 진행 데이터로 체크포인트 상태를 복원한다.
/// </summary>
public class StageController : MonoBehaviour
{
    // Start: 모든 Checkpoint의 Awake(등록)가 끝난 뒤에 실행된다.
    private void Start()
    {
        RestoreProgress();
    }

    private void RestoreProgress()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[StageController] SaveManager가 없어서 진행 상황을 복원하지 못했습니다.");
            return;
        }

        string stageName = SceneManager.GetActiveScene().name;
        ProgressData progress = SaveManager.Instance.Progress;

        if (progress.StageID != stageName) // 다른 스테이지의 기록
        {
            SaveManager.Instance.SaveProgress(stageName, -1);
            return;
        }

        if (progress.CheckpointID < 0) return; // 도달한 체크포인트 없음

        PlayerController player = FindFirstObjectByType<PlayerController>();
        Checkpoint.TryRestore(progress.CheckpointID, player);
    }
}
