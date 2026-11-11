using System;
using System.Collections.Generic;

[Serializable]
public class ProgressData
{
    public const string FILE_NAME = "Progress.json";

    public string StageID = "";
    public int CheckpointID = -1;
    public List<string> CompletedTutorials = new List<string>(); // 끝낸 튜토리얼 ID (새 게임에서 진행 데이터가 지워지면 같이 초기화된다)
}
