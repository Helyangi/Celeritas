using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraZoneManager : MonoBehaviour
{
    private CinemachineCamera _currentCamera;
    public static CameraZoneManager Instance { get; private set; }

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
    
    public void SwitchCamera(CinemachineCamera newCamera)
    {
        if (newCamera == null || _currentCamera == newCamera) return;

        if (_currentCamera != null)
            _currentCamera.Priority = 0;
        
        _currentCamera = newCamera;
        _currentCamera.Priority = 10;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
