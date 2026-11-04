using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButton : MonoBehaviour
{
    public void StartNewGame(string newSceneName)
    {
        SaveManager.Instance.ResetProgress();   // 이전 진행 기록 삭제 후 처음부터 시작
        SceneManager.LoadScene(newSceneName);
    }
}
