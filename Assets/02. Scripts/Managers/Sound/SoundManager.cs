using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private SFXObject _sfxObject;
    [SerializeField] private int _initialPoolSize = 10;

    [Header("SFX Mappings")]
    [SerializeField] private List<SFXData> _sfxList;
    private Dictionary<SFXType, SFXData> _sfxDictionary = new Dictionary<SFXType, SFXData>();

    [Header("BGM")]
    [SerializeField] private AudioSource _bgmSource;      // 배경음악 재생기 (교차 페이드를 위해 하나 더 복제해서 쓴다)
    [SerializeField] private float _defaultFade = 1f;     // 곡을 바꿀 때 겹치는 시간(초)

    private AudioSource _bgmSourceB;  // 교차 페이드용 두 번째 재생기
    private AudioSource _activeBgm;   // 지금 울리고 있는 쪽
    private Coroutine _bgmRoutine;
    private float _bgmVolume = 1f;    // 프리팹에 설정된 기준 음량

    private Queue<SFXObject> _sfxPool = new Queue<SFXObject>();
    private Transform _poolContainer;
    public static SoundManager Instance {get; private set;}

    public AudioClip CurrentBGM => _activeBgm != null ? _activeBgm.clip : null;

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

        InitDictionary();
        InitPool();
        InitBGM();
    }
    public void PlaySFX(SFXType type, Vector2 spawnPos, float pitch = 1f)
    {
        if (!_sfxDictionary.TryGetValue(type, out SFXData data))
        {
            Debug.Log($"[SoundManager] 등록되지 않은 SFX 타입입니다: {type}");
            return;
        }

        SFXObject obj = null;

        if (_sfxPool.Count > 0)
        {
            obj = _sfxPool.Dequeue();
        }
        else
        {
            obj = CreateNewSFXObject();
        }

        obj.Play(data, spawnPos, pitch);
    }

    /// <summary>
    /// 배경음악을 바꾼다. 지금 울리는 곡과 같으면 끊지 않고 그대로 이어서 재생한다.
    /// clip이 null이면 음악을 끈다. fade가 음수면 기본 페이드 시간을 쓴다.
    /// </summary>
    public void PlayBGM(AudioClip clip, float fade = -1f)
    {
        if (fade < 0f) fade = _defaultFade;

        if (_activeBgm != null && _activeBgm.clip == clip && _activeBgm.isPlaying) return; // 같은 곡이면 이어서
        if (clip == null && _activeBgm == null) return;

        if (_bgmRoutine != null) StopCoroutine(_bgmRoutine);
        _bgmRoutine = StartCoroutine(SwitchBGM(clip, fade));
    }

    public void StopBGM(float fade = -1f)
    {
        PlayBGM(null, fade);
    }

    private void InitBGM()
    {
        if (_bgmSource == null) _bgmSource = GetComponent<AudioSource>();
        if (_bgmSource == null) return;

        _bgmVolume = _bgmSource.volume;
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;
        _bgmSource.Stop();

        // 같은 설정의 재생기를 하나 더 만들어 두 개를 번갈아 쓴다 (한쪽이 사라지는 동안 다른 쪽이 올라온다)
        _bgmSourceB = gameObject.AddComponent<AudioSource>();
        _bgmSourceB.playOnAwake = false;
        _bgmSourceB.loop = true;
        _bgmSourceB.volume = _bgmVolume;
        _bgmSourceB.outputAudioMixerGroup = _bgmSource.outputAudioMixerGroup;
        _bgmSourceB.spatialBlend = _bgmSource.spatialBlend;
        _bgmSourceB.priority = _bgmSource.priority;
    }

    private IEnumerator SwitchBGM(AudioClip clip, float fade)
    {
        AudioSource from = _activeBgm;
        AudioSource to = from == _bgmSource ? _bgmSourceB : _bgmSource;

        if (clip != null)
        {
            to.clip = clip;
            to.volume = fade > 0f ? 0f : _bgmVolume;
            to.Play();
        }

        if (fade > 0f)
        {
            float elapsed = 0f;
            while (elapsed < fade)
            {
                elapsed += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(elapsed / fade);
                if (from != null) from.volume = _bgmVolume * (1f - k);
                if (clip != null) to.volume = _bgmVolume * k;
                yield return null;
            }
        }

        if (from != null)
        {
            from.Stop();
            from.clip = null;
            from.volume = _bgmVolume;
        }

        if (clip != null)
        {
            to.volume = _bgmVolume;
            _activeBgm = to;
        }
        else
        {
            _activeBgm = null;
        }

        _bgmRoutine = null;
    }

    private void InitDictionary()
    {
        _poolContainer = new GameObject("PoolContainer").transform;
        _poolContainer.SetParent(gameObject.transform);

        foreach (SFXData data in _sfxList)
        {
            if (!_sfxDictionary.TryAdd(data.Type, data))
            {
                Debug.Log($"[SoundManager] SFXType 중복: {data.Type}");
            }
        }
    }
    private void InitPool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            SFXObject obj = CreateNewSFXObject();
            obj.ReturnToPool();
        }
    }
    private SFXObject CreateNewSFXObject()
    {
        SFXObject obj = Instantiate(_sfxObject, _poolContainer);
        obj.gameObject.SetActive(false);
        obj.Init(_sfxPool);
        return obj;
    }
}
