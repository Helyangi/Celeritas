using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    // ID로 체크포인트를 찾기 위한 등록부 (진행 데이터 복원에 사용)
    private static readonly Dictionary<int, Checkpoint> _registry = new Dictionary<int, Checkpoint>();
    private static Checkpoint _currentCheckPoint;

    [SerializeField] private int _id;   // 스테이지 안에서 고유한 번호. 한 번 정하면 바꾸지 않기
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Animator _anim;
    [SerializeField] private Light2D _light;
    [SerializeField] private ParticleSystem _burst; // 저장될 때 팡 터지는 파티클 (자식 SaveBurst, Emission은 꺼두고 코드로만 생성)
    [SerializeField] private int _burstCount = 30;   // 한 번에 터뜨리는 파티클 개수
    [SerializeField] private float _enableIntensity = 1f;
    private float _disableIntensity;
    private bool _isSave = false;

    public int ID => _id;

    private void Awake()
    {
        // 같은 씬 안에서 ID가 겹치면 경고 (새로 복제한 체크포인트는 ID가 0으로 겹치기 쉬움)
        if (_registry.TryGetValue(_id, out Checkpoint existing) && existing != null
            && existing != this && existing.gameObject.scene == gameObject.scene)
        {
            Debug.LogWarning($"[Checkpoint] ID가 중복되었습니다: {_id} ({existing.name}, {name})", this);
        }
        _registry[_id] = this;

        // 꺼진 상태의 밝기를 기억. 복원이 Start 시점에 실행될 수 있어서 Awake에서 읽는다.
        _disableIntensity = _light.intensity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player") || _isSave) return;

        PlayerController controller = other.gameObject.GetComponent<PlayerController>();
        if (controller == null) return;

        controller.SetCheckpoint(respawnPoint.position);
        OnSave();
        SaveManager.Instance.SaveProgress(SceneManager.GetActiveScene().name, _id);
    }

    /// <summary>
    /// 저장된 ID의 체크포인트를 켜진 상태로 만들고 플레이어를 그 위치로 옮긴다. (소리/파티클 없이)
    /// </summary>
    public static bool TryRestore(int id, PlayerController player)
    {
        if (player == null) return false;

        if (!_registry.TryGetValue(id, out Checkpoint checkpoint) || checkpoint == null)
        {
            Debug.LogWarning($"[Checkpoint] 복원할 체크포인트를 찾지 못했습니다. ID: {id}");
            return false;
        }

        Vector3 position = checkpoint.respawnPoint.position;
        player.SetCheckpoint(position);
        player.transform.position = position;
        checkpoint.OnSave(false);
        return true;
    }

    private void OnSave(bool playEffects = true)
    {
        _currentCheckPoint?.OffSave();
        _currentCheckPoint = this;
        _isSave = true;
        _anim.Play("On");
        _light.intensity = _enableIntensity;

        if (!playEffects) return;
        SoundManager.Instance.PlaySFX(SFXType.Checkpoint, transform.position);
        if (_burst != null) _burst.Emit(_burstCount); // 등불이 켜지는 순간 한 번만 팡 (_isSave 덕분에 다시 호출되지 않음)
    }

    private void OffSave()
    {
        _anim.Play("Off");
        _isSave = false;
        _light.intensity = _disableIntensity;
    }

    private void OnDestroy()
    {
        if (_registry.TryGetValue(_id, out Checkpoint registered) && registered == this)
            _registry.Remove(_id);

        if (_currentCheckPoint == this)
            _currentCheckPoint = null;
    }
}
