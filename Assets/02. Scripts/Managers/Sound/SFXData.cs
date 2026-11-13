using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SFXData", menuName = "Data/SFXData")]
public class SFXData : ScriptableObject
{
    public SFXType Type;
    public AudioClip Clip;
    [Range(0f, 1f)]
    public float Volume = 0.5f;
    public AudioMixerGroup MixerGroup;
}
