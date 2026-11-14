using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ContinueButton : MonoBehaviour
{
    [Tooltip("저장 데이터가 없을 때 글자를 얼마나 흐리게 할지")]
    [SerializeField, Range(0f, 1f)] private float _disabledTextAlpha = 0.35f;

    private Button _button;
    private TextMeshProUGUI _label;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _label = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    // SaveManager가 먼저 준비되도록 Start에서 확인한다
    private void Start()
    {
        Refresh();
    }

    /// <summary>저장 데이터가 없으면 누를 수 없게 하고 흐리게 보여준다.</summary>
    public void Refresh()
    {
        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.HasProgress();

        if (_button != null) _button.interactable = hasSave; // 버튼 이미지는 ColorTint가 알아서 흐려준다

        if (_label != null)
        {
            Color color = _label.color;
            color.a = hasSave ? 1f : _disabledTextAlpha; // 글자는 대상 그래픽이 아니라 직접 흐리게 한다
            _label.color = color;
        }
    }

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
