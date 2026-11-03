using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("대사 & 이름")]
    [SerializeField] private string[] dialogues;
    [SerializeField] private string name;
    
    [Header("상호작용 안내 UI")]
    [SerializeField] private GameObject interactionPanel;
    
    private bool _isPlayerInRange;

    private void Update()
    {
        interactionPanel.SetActive(
            _isPlayerInRange && !DialogueManager.Instance.IsTalking);
        
        if (InputManager.Instance.IsInteract && _isPlayerInRange)
        {
            DialogueManager.Instance.StartDialogue(dialogues, name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            interactionPanel.SetActive(true);
            _isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            interactionPanel.SetActive(false);
            _isPlayerInRange = false;
        }
    }
}
