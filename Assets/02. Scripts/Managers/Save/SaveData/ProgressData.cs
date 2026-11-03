using System;

[Serializable]
public class ProgressData
{
    public const string FILE_NAME = "Progress.json";

    public string StageID = "";
    public int CheckpointID = -1;
}
