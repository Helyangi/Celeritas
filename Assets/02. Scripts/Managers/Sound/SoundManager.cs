using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{   
    [Header("Pool Settings")]
    [SerializeField] private SFXObject _sfxObject;
    [SerializeField] private int _initialPoolSize = 10;

    [Header("SFX Mappings")]
    [SerializeField] private List<SFXData> _sfxMappings;
    private Dictionary<SFXType, SFXData> _sfxDictionary;

    private Queue<SFXObject> _sfxPool = new Queue<SFXObject>();
    private Transform poolContainer;
    public static SoundManager Instance {get; private set;}
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
        }
    }
    public void PlaySFX(SFXType type, Transform transform)
    {
        
    }
}