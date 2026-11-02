using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    private string[] _dialogues;
    private int _currentIndex;
    private bool _isTalking;
    
    public static DialogueManager Instance { get; private set; }

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!_isTalking) return;
        
        if (InputManager.Instance.IsPressedNextDialogue)
        {
            NextDialogue();
        }
    }

    public void StartDialogue(string[] dialogues)
    {
        if (dialogues == null || dialogues.Length == 0 || _isTalking)
        {
            return;
        }
        
        _currentIndex = 0;
        _dialogues = dialogues;
        _isTalking = true;
        
        Debug.Log(_dialogues[_currentIndex]);
        
        InputManager.Instance.EnterDialogueMode();
    }

    public void NextDialogue()
    {
        if (!_isTalking) return;

        if (_currentIndex < _dialogues.Length - 1)
        {
            _currentIndex++;
            Debug.Log(_dialogues[_currentIndex]);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        _isTalking = false;
        
        InputManager.Instance.ExitDialogueMode();
    }
}
