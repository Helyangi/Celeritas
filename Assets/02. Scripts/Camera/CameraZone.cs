using Unity.Cinemachine;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    [SerializeField] private CinemachineCamera camera;

    // CameraZoneManager가 "이 위치가 어느 구역인가"를 물어볼 때 쓴다
    public CinemachineCamera ZoneCamera => camera;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CameraZoneManager.Instance.SwitchCamera(camera);
        }
    }
}
