using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private string[] dialogues;
    [SerializeField] private string name;
    [SerializeField] private GameObject interactionPanel;
    
    private bool _isPlayerInRange;

    private void Update()
    {
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
