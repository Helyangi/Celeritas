using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance {get; private set;}

    public SettingsData Settings {get; private set;}
    public ProgressData Progress {get; private set;}
    public event Action<SettingsData> OnSettingsChanged;
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

        Settings = SaveSystem.Load<SettingsData>(SettingsData.FILE_NAME);
        Progress = SaveSystem.Load<ProgressData>(ProgressData.FILE_NAME);
    }

    public void ApplySettings()
    {
        OnSettingsChanged?.Invoke(Settings);
    }

    public void SaveSettings()
    {
        SaveSystem.Save(SettingsData.FILE_NAME, Settings);
    }

    public void SaveProgress(string stageId, int checkpointIndex)
    {
        Progress.StageID = stageId;
        Progress.CheckpointID = checkpointIndex;

        SaveSystem.Save(ProgressData.FILE_NAME, Progress);
    }

    public void ResetSettings()
    {
        Settings = new SettingsData();
        SaveSystem.Delete(SettingsData.FILE_NAME);
        ApplySettings();
    }

    // 저장된 진행이 있는지 확인한다. 새 게임 경고를 띄울지 판단할 때 쓴다
    // 체크포인트에 도달했으면 진행이 있는 것이고, Stage1에서 체크포인트가 -1이면(튜토리얼만 한 경우 포함) 진행이 없는 것으로 본다
    public bool HasProgress()
    {
        if (Progress == null) return false;
        if (Progress.CheckpointID >= 0) return true;
        return !string.IsNullOrEmpty(Progress.StageID) && Progress.StageID != "Stage1";
    }

    public bool IsTutorialCompleted(string id)
    {
        return Progress.CompletedTutorials != null && Progress.CompletedTutorials.Contains(id);
    }

    public void MarkTutorialCompleted(string id)
    {
        if (Progress.CompletedTutorials == null) Progress.CompletedTutorials = new List<string>(); // 예전 저장 파일에는 이 필드가 없다
        if (Progress.CompletedTutorials.Contains(id)) return;

        Progress.CompletedTutorials.Add(id);
        SaveSystem.Save(ProgressData.FILE_NAME, Progress);
    }

    public void ResetProgress()
    {
        Progress = new ProgressData();
        SaveSystem.Delete(ProgressData.FILE_NAME);
    }
}
