using Unity.Cinemachine;
using UnityEngine;

public class CameraZoneManager : MonoBehaviour
{
    private CinemachineCamera _currentCamera;
    
    public void SwitchCamera(CinemachineCamera newCamera)
    {
        if (newCamera == null || _currentCamera == newCamera) return;

        if (_currentCamera != null)
            _currentCamera.Priority = 0;
        
        _currentCamera = newCamera;
        _currentCamera.Priority = 10;
    }
}
