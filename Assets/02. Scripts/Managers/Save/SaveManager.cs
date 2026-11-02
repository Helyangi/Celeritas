using System;
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
        Progress.CheckpointIndex = checkpointIndex;

        SaveSystem.Save(ProgressData.FILE_NAME, Progress);
    }

    public void ResetSettings()
    {
        Settings = new SettingsData();
        SaveSystem.Delete(SettingsData.FILE_NAME);
    }

    public void ResetProgress()
    {
        Progress = new ProgressData();
        SaveSystem.Delete(ProgressData.FILE_NAME);
    }
}
