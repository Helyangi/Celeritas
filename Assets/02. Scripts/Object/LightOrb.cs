using System.Collections;
using UnityEngine;

// 빛 오브. 플레이어가 도착하면 파티클이 터지고, 지정한 시간 동안 비활성화된다.
// 비활성화 동안: 조준선이 닿지 않고(콜라이더 꺼짐) 탈 수 없으며, 센서(LightSensor)의 발광도 꺼진 채로 고정된다.
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(LightSensor))]
public class LightOrb : MonoBehaviour
{
    [Header("비활성화")]
    [SerializeField] private float _disableTime = 1.5f; // 도착한 뒤 다시 탈 수 있게 되기까지의 시간(초). 오브마다 따로 지정 가능

    [Header("도착 연출")]
    [SerializeField] private ParticleSystem _arriveBurst;
    [SerializeField] private int _burstCount = 18;

    [Header("비활성화 중 모습")]
    [SerializeField] private SpriteRenderer _sprite;                // 오브 본체 (어둡게 보이게 한다)
    [SerializeField, Range(0f, 1f)] private float _disabledAlpha = 0.3f;
    [SerializeField] private float _disabledLightIntensity = 0.1f;  // 비활성화 중 센서 발광 밝기

    private Collider2D _collider;
    private LightSensor _sensor;
    private Coroutine _routine;

    public bool IsDisabled { get; private set; }

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _sensor = GetComponent<LightSensor>();
    }

    // 플레이어가 이 오브에 도착했을 때 불린다
    public void OnArrived()
    {
        if (IsDisabled) return;

        if (_arriveBurst != null)
        {
            _arriveBurst.Play();
            _arriveBurst.Emit(_burstCount);
        }

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(DisableRoutine());
    }

    private IEnumerator DisableRoutine()
    {
        SetDisabled(true);
        yield return new WaitForSeconds(_disableTime);
        SetDisabled(false);
        _routine = null;
    }

    private void SetDisabled(bool disabled)
    {
        IsDisabled = disabled;
        _collider.enabled = !disabled; // 꺼두면 조준선(박스캐스트)에 닿지 않아 탈 수 없다

        if (disabled) _sensor.Lock(_disabledLightIntensity);
        else _sensor.Unlock();

        if (_sprite != null)
        {
            Color c = _sprite.color;
            c.a = disabled ? _disabledAlpha : 1f;
            _sprite.color = c;
        }
    }

    private void OnDisable()
    {
        // 비활성화 도중 오브젝트가 꺼지거나 씬이 정리돼도 꺼진 상태로 남지 않게 되돌린다
        if (IsDisabled && _collider != null) SetDisabled(false);
    }
}
