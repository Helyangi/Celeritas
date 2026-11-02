using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private string[] dialogues;
    private bool _isPlayerInRange;

    private void Update()
    {
        if (InputManager.Instance.IsInteract && _isPlayerInRange)
        {
            DialogueManager.Instance.StartDialogue(dialogues);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
        }
    }
}
