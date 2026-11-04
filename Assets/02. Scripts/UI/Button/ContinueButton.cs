using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueButton : MonoBehaviour
{
    public void ContinueGame()
    {
        string stageId = SaveManager.Instance.Progress.StageID;

        if (string.IsNullOrEmpty(stageId))
        {
            Debug.LogWarning("[ContinueButton] 저장된 진행 상황이 없습니다.");
            return;
        }

        SceneManager.LoadScene(stageId);
    }
}
