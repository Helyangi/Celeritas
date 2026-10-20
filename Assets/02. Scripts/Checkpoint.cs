using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController controller = other.gameObject.GetComponent<PlayerController>();
            if (controller == null) return;
            controller.SetCheckpoint(respawnPoint.position);
        }
    }
}
