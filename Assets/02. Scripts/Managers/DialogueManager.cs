using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private float waitTime;
    
    [Header("대화창")]
    [SerializeField] private GameObject dialoguesPanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    
    [Header("플레이어 오브젝트")]
    [SerializeField] private PlayerController controller;
    
    private string[] _dialogues;
    private int _currentIndex;
    private bool _isTalking;
    private string _npcName;
    private bool _isTyping;
    private Coroutine _typingCoroutine;
    
    public bool IsTalking => _isTalking;
    
    public static DialogueManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        if (dialogues == null || dialogues.Length == 0 || _isTalking || npcName == null || controller == null)
        {
            return;
        }
        
        _currentIndex = 0;
        _dialogues = dialogues;
        _isTalking = true;
        _npcName = npcName;
        
        // 이름 & 대사 띄우기
        nameText.text = _npcName;
        if (!_isTyping)
            _typingCoroutine = StartCoroutine(TypeDialogue(_currentIndex));
        
        InputManager.Instance.EnterDialogueMode();
        dialoguesPanel.SetActive(true);
        controller.LightAimingLine.SetAiming(false);
    }

    public void NextDialogue()
    {
        if (!_isTalking) return;

        if (_isTyping)
        {
            StopCoroutine(_typingCoroutine);
            dialogueText.maxVisibleCharacters = int.MaxValue;
            _isTyping = false;
            return;
        }

        if (_currentIndex < _dialogues.Length - 1)
        {
            _currentIndex++;
            _typingCoroutine = StartCoroutine(TypeDialogue(_currentIndex));
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
        controller.LightAimingLine.SetAiming(true);
    }

    private IEnumerator TypeDialogue(int index)
    {
        dialogueText.text = _dialogues[index];
        _isTyping = true;

        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i < dialogueText.text.Length; i++)
        {
            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(waitTime);
        }
        _isTyping = false;
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        controller = player != null ? player.GetComponent<PlayerController>() : null;
        
        _currentIndex = 0;
        _isTalking = false;
        _isTyping = false;
        
        dialoguesPanel.SetActive(false);
        
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        
        if (InputManager.Instance != null)
            InputManager.Instance.ExitDialogueMode();
    }
}
