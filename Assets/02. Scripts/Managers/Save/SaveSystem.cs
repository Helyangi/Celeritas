using System;
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string GetPath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }
    public static void Save<T>(string fileName, T data)
    {
        string path = GetPath(fileName);
        string tempPath = path + ".temp";
        
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(tempPath, json);

            if (File.Exists(path)) File.Replace(tempPath, path, null);
            else File.Move(tempPath, path);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 저장 실패 ({fileName}): {e.Message}");
        }
    }
    public static T Load<T>(string fileName) where T : new()
    {
        string path = GetPath(fileName);

        if (!File.Exists(path))
        {
            return new T();
        }
        try
        {
            string json = File.ReadAllText(path);
            T data = JsonUtility.FromJson<T>(json);
            return data != null ? data : new T();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] 불러오기 실패 ({fileName}): {e.Message}");
            return new T();
        }
    }
    public static void Delete(string fileName)
    {
        string path = GetPath(fileName);
        if (File.Exists(path)) File.Delete(path);
    }
}
