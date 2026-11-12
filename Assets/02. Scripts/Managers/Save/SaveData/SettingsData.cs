using System;

[Serializable]
public class SettingsData
{
    public const string FILE_NAME = "Settings.json";

    public float MasterVolume = 1f;
    public float BGMVolume = 1f;
    public float SFXVolume = 1f;
}
