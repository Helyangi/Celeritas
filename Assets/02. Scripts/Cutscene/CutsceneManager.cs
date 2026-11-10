using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 컷씬(CutsceneData)을 받아 단계를 위에서부터 순서대로 실행하는 매니저.
/// - 컷씬이 도는 동안 플레이어 조작을 막고, 끝나면 풀어준다
/// - 씬을 넘어가도 유지된다 (씬 이동 단계 뒤에도 컷씬이 이어질 수 있도록)
/// - 단계에서 오류가 나면 그 단계만 건너뛰고 계속 진행한다 (컷씬이 멈춘 채로 조작이 잠기지 않도록)
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance { get; private set; }

    [SerializeField] private ScreenFader _fader;                // 페이드 단계가 쓰는 화면 가림막
    [SerializeField] private int _cutsceneCameraPriority = 100; // 컷씬 카메라 우선순위 (구역 카메라의 10보다 높게)

    private CutsceneData _current;
    private CinemachineCamera _activeCamera; // 컷씬이 켜 둔 카메라 (끝나면 원래 우선순위로 돌려놓는다)
    private int _activeCameraOriginalPriority;

    public bool IsPlaying => _current != null;
    public CutsceneData Current => _current;
    public ScreenFader Fader => _fader;

    public event Action<CutsceneData> CutsceneStarted;
    public event Action<CutsceneData> CutsceneEnded;

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

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// 컷씬을 재생한다. 이미 다른 컷씬이 재생 중이면 시작하지 않고 false.
    /// </summary>
    public bool Play(CutsceneData data)
    {
        if (data == null)
        {
            Debug.LogWarning("[CutsceneManager] 재생할 컷씬이 비어 있습니다.");
            return false;
        }

        if (IsPlaying)
        {
            Debug.LogWarning($"[CutsceneManager] '{_current.Id}' 재생 중이라 '{data.Id}'를 시작하지 않았습니다.");
            return false;
        }

        _current = data;
        StartCoroutine(PlayRoutine(data));
        return true;
    }

    private IEnumerator PlayRoutine(CutsceneData data)
    {
        LockPlayerInput();
        CutsceneStarted?.Invoke(data);

        IReadOnlyList<CutsceneStep> steps = data.Steps;
        for (int i = 0; i < steps.Count; i++)
        {
            CutsceneStep step = steps[i];
            if (step == null) continue; // 비어 있는 칸은 건너뛴다

            yield return RunStep(step, data, i);
        }

        Finish(data);
    }

    // 단계 하나를 끝까지 실행한다. 단계 안에서 오류가 나면 기록만 남기고 그 단계를 끝낸다.
    private IEnumerator RunStep(CutsceneStep step, CutsceneData data, int index)
    {
        IEnumerator routine = null;
        try
        {
            routine = step.Execute(this);
        }
        catch (Exception e)
        {
            LogStepError(e, data, index, step);
        }
        if (routine == null) yield break;

        while (true)
        {
            bool hasNext = false;
            object current = null;
            try
            {
                hasNext = routine.MoveNext();
                if (hasNext) current = routine.Current;
            }
            catch (Exception e)
            {
                LogStepError(e, data, index, step);
            }

            if (!hasNext) yield break;
            yield return current;
        }
    }

    private void Finish(CutsceneData data)
    {
        ReleaseCamera();
        _current = null;
        UnlockPlayerInput();
        CutsceneEnded?.Invoke(data);
    }

    // 컷씬 동안 플레이어 조작을 막는다. 씬 이동처럼 다른 매니저가 입력을 다시 켤 수 있는 단계 뒤에 다시 호출한다.
    public void LockPlayerInput()
    {
        if (InputManager.Instance != null) InputManager.Instance.DisablePlayerInput();
    }

    private void UnlockPlayerInput()
    {
        if (InputManager.Instance != null) InputManager.Instance.EnablePlayerInput();
    }

    /// <summary>
    /// 컷씬 카메라로 전환한다. 이전에 컷씬이 켜 둔 카메라는 원래 우선순위로 돌려놓는다.
    /// </summary>
    public void FocusCamera(CinemachineCamera camera)
    {
        if (camera == _activeCamera) return;

        ReleaseCamera();
        if (camera == null) return;

        if (!camera.isActiveAndEnabled)
        {
            Debug.LogWarning($"[CutsceneManager] 꺼져 있는 카메라라 전환되지 않을 수 있습니다: {camera.name}", camera);
        }

        _activeCamera = camera;
        _activeCameraOriginalPriority = camera.Priority;
        camera.Priority = _cutsceneCameraPriority;
    }

    /// <summary>
    /// 컷씬 카메라를 원래대로 돌려놓아, 평소 카메라(구역 카메라 등)로 돌아가게 한다.
    /// </summary>
    public void ReleaseCamera()
    {
        if (_activeCamera != null) _activeCamera.Priority = _activeCameraOriginalPriority;
        _activeCamera = null;
    }

    private static void LogStepError(Exception e, CutsceneData data, int index, CutsceneStep step)
    {
        Debug.LogError($"[CutsceneManager] '{data.Id}' {index + 1}번 단계({step.GetType().Name})에서 오류가 나서 건너뜁니다.\n{e}");
    }
}
