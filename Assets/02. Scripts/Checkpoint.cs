using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Checkpoint : MonoBehaviour
{
    private static Checkpoint _currentCheckPoint;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Animator _anim;
    [SerializeField] private Light2D _light;
    [SerializeField] private ParticleSystem _burst; // 저장될 때 팡 터지는 파티클 (자식 SaveBurst, Emission은 꺼두고 코드로만 생성)
    [SerializeField] private int _burstCount = 30;   // 한 번에 터뜨리는 파티클 개수
    private bool _isSave = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !_isSave)
        {
            OnSave();
            PlayerController controller = other.gameObject.GetComponent<PlayerController>();
            if (controller == null) return;
            controller.SetCheckpoint(respawnPoint.position);

        }
    }
    private void OnSave()
    {
        _currentCheckPoint?.OffSave();
        _currentCheckPoint = this;
        _isSave = true;
        SoundManager.Instance.PlaySFX(SFXType.Checkpoint, transform.position);
        _anim.Play("On");
        _light.intensity = 1;
        if (_burst != null) _burst.Emit(_burstCount); // 등불이 켜지는 순간 한 번만 팡 (_isSave 덕분에 다시 호출되지 않음)
    }
    private void OffSave()
    {
        _isSave = false;
        _light.intensity = 0;
    }
}
