using UnityEngine;

public class DeadLine : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            SoundManager.Instance.PlaySFX(SFXType.Death, transform.position);
            PlayerController controller = other.gameObject.GetComponent<PlayerController>();
            if (controller == null) return;
            controller.Respawn();
        }
    }
}
