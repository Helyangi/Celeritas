using Unity.Cinemachine;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField] private CinemachineCamera camera;
    [SerializeField] private CameraZoneManager manager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            manager.SwitchCamera(camera);
        }
    }
}
