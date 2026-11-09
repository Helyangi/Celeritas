using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraZoneManager : MonoBehaviour
{
    private CinemachineCamera _currentCamera;
    public static CameraZoneManager Instance { get; private set; }

    // 순간 전환(Cut) 처리용
    private CinemachineBrain _brain;
    private CinemachineBlendDefinition _originalBlend;
    private bool _isCutting;
    private Coroutine _restoreRoutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        if (Instance != this) return;
        PlayerController.Spawned += OnPlayerSpawned;
    }

    private void OnDisable()
    {
        PlayerController.Spawned -= OnPlayerSpawned;
    }

    // 평소 구역 이동: 기존처럼 부드럽게 블렌드된다
    public void SwitchCamera(CinemachineCamera newCamera)
    {
        SwitchCamera(newCamera, false);
    }

    // snap이 true면 블렌드 없이 즉시 전환하고, 새 카메라의 Damping 상태도 버린다
    public void SwitchCamera(CinemachineCamera newCamera, bool snap)
    {
        if (newCamera == null || _currentCamera == newCamera) return;

        if (_currentCamera != null)
            _currentCamera.Priority = 0;
        
        _currentCamera = newCamera;
        _currentCamera.Priority = 10;

        if (snap)
        {
            _currentCamera.PreviousStateIsValid = false; // 이전 위치에서 따라오지 않고 바로 타겟에 붙는다
            CutNextBlend();
        }
    }

    // 플레이어가 게임 시작/리스폰으로 등장했을 때: 그 위치의 구역 카메라로 즉시 전환한다
    private void OnPlayerSpawned(Transform player, Vector3 delta)
    {
        CinemachineCamera zoneCamera = FindZoneCamera(player.position);

        if (zoneCamera != null && zoneCamera != _currentCamera)
        {
            SwitchCamera(zoneCamera, true);
        }
        else if (_currentCamera != null)
        {
            // 같은 구역 안에서 순간이동한 경우: Damping 때문에 끌려오지 않도록 카메라도 같이 옮겨준다
            _currentCamera.OnTargetObjectWarped(player, delta);
        }
    }

    private CinemachineCamera FindZoneCamera(Vector3 position)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(position);
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out CameraZone zone) && zone.ZoneCamera != null)
                return zone.ZoneCamera;
        }
        return null;
    }

    // 다음 카메라 전환 한 번만 Cut으로 만들고 원래 블렌드 설정으로 되돌린다
    private void CutNextBlend()
    {
        if (_brain == null) _brain = FindFirstObjectByType<CinemachineBrain>();
        if (_brain == null) return;

        if (!_isCutting)
        {
            _originalBlend = _brain.DefaultBlend;
            _isCutting = true;
        }
        _brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Cut, 0f);

        if (_restoreRoutine != null) StopCoroutine(_restoreRoutine);
        _restoreRoutine = StartCoroutine(RestoreBlend(_brain));
    }

    private IEnumerator RestoreBlend(CinemachineBrain brain)
    {
        // 브레인이 새 카메라로의 전환을 처리할 때까지 몇 프레임 기다린다
        yield return null;
        yield return null;
        yield return null;

        if (brain != null) brain.DefaultBlend = _originalBlend;
        _isCutting = false;
        _restoreRoutine = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}