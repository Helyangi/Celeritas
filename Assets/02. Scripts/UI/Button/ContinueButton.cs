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

        // 메뉴에 MenuFade가 있으면 화면이 어두워진 뒤에 넘어간다 (없으면 지금까지처럼 바로 넘어간다)
        if (MenuFade.Instance != null) MenuFade.Instance.LoadScene(stageId);
        else SceneManager.LoadScene(stageId);
    }
}
