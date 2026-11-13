using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 스테이지 씬마다 하나씩 두는 컴포넌트.
/// 씬이 시작될 때 저장된 진행 데이터로 체크포인트 상태를 복원하고, 플레이어를 등장시킨다.
/// </summary>
public class StageController : MonoBehaviour
{
    // Start: 모든 Checkpoint의 Awake(등록)가 끝난 뒤에 실행된다.
    private void Start()
    {
        StartStage();
    }

    private void StartStage()
    {
        InputManager.Instance.EnablePlayerInput();
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            Debug.LogWarning("[StageController] PlayerController를 찾지 못했습니다.");
            return;
        }

        // 저장된 체크포인트가 있으면 그 위치에서 등장한다 (Checkpoint.TryRestore가 Spawn까지 호출)
        if (RestoreProgress(player)) return;

        // 복원할 체크포인트가 없으면(처음 시작 등) 씬에 놓인 위치에서 등장한다
        player.Spawn(player.transform.position);
    }

    // 체크포인트에서 복원해 플레이어를 등장시켰으면 true
    private bool RestoreProgress(PlayerController player)
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[StageController] SaveManager가 없어서 진행 상황을 복원하지 못했습니다.");
            return false;
        }

        string stageName = SceneManager.GetActiveScene().name;
        ProgressData progress = SaveManager.Instance.Progress;

        if (progress.StageID != stageName) // 다른 스테이지의 기록
        {
            SaveManager.Instance.SaveProgress(stageName, -1);
            return false;
        }

        if (progress.CheckpointID < 0) return false; // 도달한 체크포인트 없음

        return Checkpoint.TryRestore(progress.CheckpointID, player);
    }
}
