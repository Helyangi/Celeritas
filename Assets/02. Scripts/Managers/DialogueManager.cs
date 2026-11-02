using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguesPanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    
    private string[] _dialogues;
    private int _currentIndex;
    private bool _isTalking;
    private string _npcName;
    
    public static DialogueManager Instance { get; private set; }

    private void Awake()
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

    public void StartDialogue(string[] dialogues, string npcName)
    {
        if (dialogues == null || dialogues.Length == 0 || _isTalking || npcName == null)
        {
            return;
        }
        
        _currentIndex = 0;
        _dialogues = dialogues;
        _isTalking = true;
        _npcName = npcName;
        
        nameText.text = _npcName;
        dialogueText.text = _dialogues[_currentIndex];
        
        InputManager.Instance.EnterDialogueMode();
        dialoguesPanel.SetActive(true);
    }

    public void NextDialogue()
    {
        if (!_isTalking) return;

        if (_currentIndex < _dialogues.Length - 1)
        {
            _currentIndex++;
            dialogueText.text = _dialogues[_currentIndex];
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        _isTalking = false;
        dialoguesPanel.SetActive(false);
        
        InputManager.Instance.ExitDialogueMode();
    }
}
