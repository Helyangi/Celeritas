using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSensor : MonoBehaviour
{
    public Light2D Light;
    [SerializeField] private float _enableIntensity = 0.5f;
    private float _disableIntensity;
    private bool _locked; // 잠겨 있는 동안은 발광이 켜지거나 꺼지지 않는다 (빛 오브 쿨타임용)
    public void Awake()
    {
        if (Light == null) Light = GetComponent<Light2D>();
        _disableIntensity = Light.intensity;
    }
    public void EnableLight()
    {
        if (_locked) return;
        Light.intensity = _enableIntensity;
    }
    public void DisableLight()
    {
        if (_locked) return;
        Light.intensity = _disableIntensity;
    }
    public void Lock(float intensity) // 센서를 비활성화하고 지정한 밝기로 고정한다
    {
        _locked = true;
        Light.intensity = intensity;
    }
    public void Unlock() // 비활성화를 풀고 평소 밝기로 되돌린다
    {
        _locked = false;
        Light.intensity = _disableIntensity;
    }
}
