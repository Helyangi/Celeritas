using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void OnClickEvent(string newSceneName)
    {
        SceneManager.LoadScene(newSceneName);
    }
}
