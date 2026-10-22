using Unity.Cinemachine;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField] private CinemachineCamera camera;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CameraZoneManager.Instance.SwitchCamera(camera);
        }
    }
}
