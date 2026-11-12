using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "SFXData", menuName = "Data/SFXData")]
public class SFXData : ScriptableObject
{
    public SFXType Type;
    public AudioClip Clip;
    public AudioMixerGroup MixerGroup;
}
