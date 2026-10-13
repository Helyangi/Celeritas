using UnityEngine;

[CreateAssetMenu(fileName = "SFXData", menuName = "Data/SFXData")]
public class SFXData : ScriptableObject
{
    public SFXType Type;
    public AudioClip Clip;
}
