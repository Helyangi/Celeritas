using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameButton : MonoBehaviour
{
    public void OnClickEvent(string newSceneName)
    {
        SceneManager.LoadScene(newSceneName);
    }
}
