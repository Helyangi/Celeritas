using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 죽음/등장 연출을 담당한다.
/// PlayerController의 이벤트(Died, Spawned)만 구독하며, 다른 곳에서 이 스크립트를 참조하지 않는다.
/// 지금은 스프라이트 알파와 빛 밝기를 서서히 바꾸는 임시 연출이다.
/// 가루가 되어 사라지는 애니메이션/파티클이 준비되면 OnDied, OnSpawned 안에서 그걸 재생하면 된다.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class PlayerLifeEffect : MonoBehaviour
{
    private PlayerController _player;
    private float _baseIntensity; // 평소 빛 밝기
    private Coroutine _routine;

    private void Awake()
    {
        _player = GetComponent<PlayerController>();
        if (_player.Light != null) _baseIntensity = _player.Light.intensity;
    }

    private void OnEnable()
    {
        _player.Died += OnDied;
        PlayerController.Spawned += OnSpawned;
    }

    private void OnDisable()
    {
        _player.Died -= OnDied;
        PlayerController.Spawned -= OnSpawned;
    }

    // 죽는 순간: 리스폰까지 걸리는 시간(RespawnTime) 동안 서서히 사라진다
    private void OnDied()
    {
        
    }

    // 등장: 보이지 않는 상태에서 시작해 서서히 나타난다
    private void OnSpawned(Transform player, Vector3 delta)
    {
        if (player != transform) return;
    }
}
