using UnityEditor;
using UnityEngine;

public static class SaveFolderMenu
{
    [MenuItem("Tools/Save/Open Save Folder")]
    private static void OpenSaveFolder()
    {
        string path = Application.persistentDataPath;
        EditorUtility.RevealInFinder(path);
    }
}