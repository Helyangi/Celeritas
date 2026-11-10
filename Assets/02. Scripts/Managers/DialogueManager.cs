using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// 대화창을 띄우고 대사를 한 줄씩 보여주는 매니저. NPC 대화와 컷씬 대사가 함께 쓴다.
/// - NPC: StartDialogue(대사 배열, 이름) → 대화 동안 플레이어 조작을 막고, 끝나면 풀어준다 (기존과 동일)
/// - 컷씬: StartDialogue(대사 목록, 기본 이름, controlPlayer: false) → 조작은 컷씬 매니저가 관리한다
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [SerializeField] private float waitTime;
    
    [Header("대화창")]
    [SerializeField] private GameObject dialoguesPanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    
    [Header("플레이어 오브젝트")]
    [SerializeField] private PlayerController controller;
    
    private readonly List<DialogueLine> _lines = new List<DialogueLine>();
    private string _defaultName;  // 화자가 비어 있는 줄에 표시할 이름
    private bool _controlPlayer;  // 이번 대화가 플레이어 조작을 막고 풀어주는지 (NPC: true, 컷씬: false)
    private int _currentIndex;
    private bool _isTalking;
    private bool _isTyping;
    private int _startFrame;      // 대화를 시작한 프레임 (그 프레임의 입력으로 첫 대사가 넘어가지 않도록)
    private Coroutine _typingCoroutine;
    
    public bool IsTalking => _isTalking;

    // 대화가 시작되고 끝나는 순간 알려준다 (컷씬이 "대사가 끝날 때까지" 기다릴 때 사용)
    public event Action DialogueStarted;
    public event Action DialogueEnded;
    
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
        if (Time.frameCount == _startFrame) return;
        
        if (InputManager.Instance.IsPressedNextDialogue)
        {
            NextDialogue();
        }
    }

    /// <summary>
    /// NPC용 (기존 방식). 모든 대사를 같은 이름으로 보여주고, 대화 동안 플레이어 조작을 막는다.
    /// </summary>
    public bool StartDialogue(string[] dialogues, string npcName)
    {
        if (dialogues == null || dialogues.Length == 0 || npcName == null) return false;

        List<DialogueLine> lines = new List<DialogueLine>(dialogues.Length);
        foreach (string text in dialogues)
        {
            lines.Add(new DialogueLine(null, text));
        }

        return StartDialogue(lines, npcName, true);
    }

    /// <summary>
    /// 대사 목록으로 대화를 시작한다. 시작하지 못했으면(이미 대화 중 등) false.
    /// </summary>
    /// <param name="lines">대사 목록 (줄마다 화자를 정할 수 있다)</param>
    /// <param name="defaultName">화자가 비어 있는 줄에 표시할 이름</param>
    /// <param name="controlPlayer">true면 대화 동안 플레이어 조작을 막고 끝나면 풀어준다. 호출한 쪽이 조작을 관리하면 false</param>
    public bool StartDialogue(IReadOnlyList<DialogueLine> lines, string defaultName = "", bool controlPlayer = true)
    {
        if (lines == null || lines.Count == 0) return false;

        if (_isTalking)
        {
            Debug.LogWarning("[DialogueManager] 이미 대화 중이라 새 대화를 시작하지 않았습니다.");
            return false;
        }

        _lines.Clear();
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i] != null) _lines.Add(lines[i]);
        }
        if (_lines.Count == 0) return false;
        
        _defaultName = defaultName ?? "";
        _controlPlayer = controlPlayer;
        _currentIndex = 0;
        _isTalking = true;
        _startFrame = Time.frameCount;

        if (_controlPlayer)
        {
            InputManager.Instance.EnterDialogueMode();
            if (controller != null) controller.LightAimingLine.SetAiming(false);
        }
        else
        {
            InputManager.Instance.EnableDialogueInput(); // 대사 넘기기 입력만 켠다
        }
        
        dialoguesPanel.SetActive(true);
        ShowLine(_currentIndex);
        DialogueStarted?.Invoke();
        return true;
    }

    public void NextDialogue()
    {
        if (!_isTalking) return;

        if (_isTyping)
        {
            StopTyping();
            dialogueText.maxVisibleCharacters = int.MaxValue;
            return;
        }

        if (_currentIndex < _lines.Count - 1)
        {
            _currentIndex++;
            ShowLine(_currentIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        if (!_isTalking) return;

        _isTalking = false;
        StopTyping();
        dialoguesPanel.SetActive(false);

        if (_controlPlayer)
        {
            InputManager.Instance.ExitDialogueMode();
            if (controller != null) controller.LightAimingLine.SetAiming(true);
        }
        else
        {
            InputManager.Instance.DisableDialogueInput();
        }

        DialogueEnded?.Invoke();
    }

    // 한 줄을 화면에 띄운다 (이름 / 효과음 / 타이핑)
    private void ShowLine(int index)
    {
        DialogueLine line = _lines[index];
        SpeakerData speaker = line.Speaker;

        nameText.text = speaker != null && !string.IsNullOrEmpty(speaker.DisplayName) ? speaker.DisplayName : _defaultName;

        if (speaker != null && speaker.UseVoiceSFX && SoundManager.Instance != null)
        {
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            SoundManager.Instance.PlaySFX(speaker.VoiceSFX, pos);
        }

        StopTyping();
        _typingCoroutine = StartCoroutine(TypeDialogue(line.Text ?? ""));
    }

    private IEnumerator TypeDialogue(string text)
    {
        dialogueText.text = text;
        _isTyping = true;

        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i < dialogueText.text.Length; i++)
        {
            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(waitTime);
        }
        _isTyping = false;
        _typingCoroutine = null;
    }

    private void StopTyping()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
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

        bool wasTalking = _isTalking;
        
        _currentIndex = 0;
        _isTalking = false;
        StopTyping();
        
        dialoguesPanel.SetActive(false);
        
        if (InputManager.Instance != null)
            InputManager.Instance.ExitDialogueMode();

        // 대화 도중 씬이 바뀌었다면 기다리던 쪽(컷씬 등)이 멈추지 않도록 끝났다고 알려준다
        if (wasTalking) DialogueEnded?.Invoke();
    }
}
