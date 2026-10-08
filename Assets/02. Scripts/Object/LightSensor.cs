using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSensor : MonoBehaviour
{
    public Light2D Light;
    [SerializeField] private float _enableIntensity = 0.5f;
    private float _disableIntensity;
    public void Awake()
    {
        if (Light == null) Light = GetComponent<Light2D>();
        _disableIntensity = Light.intensity;
    }
    public void EnableLight()
    {
        Light.intensity = _enableIntensity;
    }
    public void DisableLight()
    {
        Light.intensity = _disableIntensity;
    }
}
