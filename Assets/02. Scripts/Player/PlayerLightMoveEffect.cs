using Unity.Cinemachine;
using UnityEngine;

// 빛 이동 연출 컴포넌트.
// 발사: 조준선이 굵고 밝게 터지고(플래시), 파티클이 진행 반대쪽으로 흩어지고, 카메라가 반동한다
// 반사: 꺾인 지점에서 작게 튄다
// (도착 연출은 LightOrb가 자기 자리에서 터뜨리므로 여기서는 다루지 않는다)
// PlayerController의 이벤트만 구독하며, PlayerController는 이 클래스를 알지 못한다. (PlayerJumpEffect와 같은 구조)
[RequireComponent(typeof(PlayerController))]
public class PlayerLightMoveEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem _shotBurst;    // 발사하는 순간 터지는 파티클
    [SerializeField] private ParticleSystem _reflectBurst; // 거울에서 꺾이는 순간 터지는 파티클

    [Header("발사 - 파티클")]
    [SerializeField] private int _shotCount = 20;
    [SerializeField] private Vector2 _shotSpeed = new Vector2(2f, 5f); // 퍼지는 속도 (최소~최대)
    [SerializeField] private float _shotSpread = 0.12f;                // 생성 위치가 흩어지는 반경
    [SerializeField, Range(0f, 1f)] private float _shotBackward = 0.6f; // 진행 반대쪽으로 쏠리는 정도 (0이면 사방으로 고르게)

    [Header("발사 - 경로 플래시")]
    [SerializeField] private LightFlash _flash; // 조준선과 별개인 전용 LineRenderer

    [Header("카메라 반동")]
    [SerializeField] private CinemachineImpulseSource _impulse;
    [SerializeField] private float _shotImpulse = 0.3f; // 발사 반동 세기 (0이면 끔)

    [Header("거울 반사 - 파티클")]
    [SerializeField] private int _reflectCount = 6;
    [SerializeField] private Vector2 _reflectSpeed = new Vector2(1f, 2.5f);
    [SerializeField] private float _reflectSpread = 0.06f;

    private PlayerController _player;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        _player.LightShot += OnLightShot;
        _player.LightReflected += OnLightReflected;
    }

    private void OnDisable()
    {
        _player.LightShot -= OnLightShot;
        _player.LightReflected -= OnLightReflected;
    }

    private void OnLightShot(Vector2 position, Vector2 direction)
    {
        Burst(_shotBurst, position, -direction * _shotBackward, _shotCount, _shotSpeed, _shotSpread);
        Recoil(-direction, _shotImpulse); // 쏜 반대쪽으로 밀린다
        if (_flash != null) _flash.Play(position, _player.LightAimingLine.LightPath); // 쏜 경로를 복사해 따로 번쩍인다
    }

    private void OnLightReflected(Vector2 position, Vector2 direction)
    {
        Burst(_reflectBurst, position, Vector2.zero, _reflectCount, _reflectSpeed, _reflectSpread);
    }

    private void Recoil(Vector2 direction, float force)
    {
        if (_impulse == null || force <= 0f) return;
        _impulse.GenerateImpulseWithVelocity(direction * force);
    }

    // 주어진 자리에서 사방으로 터뜨린다. bias를 주면 그 방향으로 쏠린다.
    private static void Burst(ParticleSystem particle, Vector2 position, Vector2 bias, int count, Vector2 speed, float spread)
    {
        if (particle == null || count <= 0) return;

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        for (int i = 0; i < count; i++)
        {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) + bias;
            if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up; // 방향이 상쇄돼 0이 되는 드문 경우
            direction.Normalize();

            emitParams.position = position + Random.insideUnitCircle * spread;
            emitParams.velocity = direction * Random.Range(speed.x, speed.y);
            particle.Emit(emitParams, 1);
        }
    }
}
