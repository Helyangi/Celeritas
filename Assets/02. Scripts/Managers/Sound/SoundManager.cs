using System;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Serializable]
    public struct SFXMapping
    {
        public SFXType SFXType;
        public SFXData SFXData;
    }

    [Header("Pool Settings")]
    [SerializeField] private SFXObject _sfxObject;
    [SerializeField] private int _initialPoolSize = 10;

    [Header("SFX Mappings")]
    [SerializeField] private List<SFXMapping> _sfxMappings;
    private Dictionary<SFXType, SFXData> _sfxDictionary = new Dictionary<SFXType, SFXData>();

    private Queue<SFXObject> _sfxPool = new Queue<SFXObject>();
    private Transform _poolContainer;
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

        InitDictionary();

    }
    private void InitDictionary()
    {
        foreach (SFXMapping mapping in _sfxMappings)
        {
            if (!_sfxDictionary.ContainsKey(mapping.SFXType))
            {
                _sfxDictionary.Add(mapping.SFXType, mapping.SFXData);
            }
        }
    }
    private void InitPool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            SFXObject obj = CreateNewSFXObject();
        }
    }
    private SFXObject CreateNewSFXObject()
    {
        SFXObject obj = Instantiate(_sfxObject, _poolContainer);
        obj.gameObject.SetActive(false);
        return obj;
    }
    public void PlaySFX(SFXType type, Transform transform)
    {
        
    }
}