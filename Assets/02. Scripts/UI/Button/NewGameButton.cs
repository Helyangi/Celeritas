using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButton : MonoBehaviour
{
    [SerializeField] private GameObject _warningUI; // 저장 데이터가 있을 때 띄우는 경고 창

    private string _pendingScene; // 경고에서 "네"를 눌렀을 때 시작할 씬

    // 새 게임 버튼: 저장 데이터가 있으면 경고를 먼저 띄우고, 없으면 바로 시작한다
    public void RequestNewGame(string newSceneName)
    {
        _pendingScene = newSceneName;

        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasProgress();
        if (hasSave && _warningUI != null)
        {
            _warningUI.SetActive(true);
            return;
        }

        StartNewGame(newSceneName);
    }

    // 경고의 "네" 버튼
    public void ConfirmNewGame()
    {
        StartNewGame(_pendingScene);
    }

    // 경고의 "아니요" 버튼
    public void CancelNewGame()
    {
        if (_warningUI != null) _warningUI.SetActive(false);
    }

    public void StartNewGame(string newSceneName)
    {
        if (SaveManager.Instance != null) SaveManager.Instance.ResetProgress();   // 이전 진행 기록 삭제 후 처음부터 시작

        // 메뉴에 MenuFade가 있으면 화면이 어두워진 뒤에 넘어간다 (없으면 지금까지처럼 바로 넘어간다)
        if (MenuFade.Instance != null) MenuFade.Instance.LoadScene(newSceneName);
        else SceneManager.LoadScene(newSceneName);
    }
}
