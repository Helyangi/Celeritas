using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// CutsceneData 인스펙터. 단계를 메뉴에서 골라 추가하고, 순서를 바꾸거나 지울 수 있게 해 준다.
/// 새 단계 클래스를 만들면 자동으로 "+ 단계 추가" 메뉴에 나타난다.
/// </summary>
[CustomEditor(typeof(CutsceneData))]
public class CutsceneDataEditor : Editor
{
    private SerializedProperty _id;
    private SerializedProperty _steps;

    private void OnEnable()
    {
        _id = serializedObject.FindProperty("_id");
        _steps = serializedObject.FindProperty("_steps");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_id, new GUIContent("ID"));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("단계 (위에서부터 순서대로 실행)", EditorStyles.boldLabel);

        if (_steps.arraySize == 0)
        {
            EditorGUILayout.HelpBox("아래 버튼으로 단계를 추가하세요.", MessageType.Info);
        }

        for (int i = 0; i < _steps.arraySize; i++)
        {
            if (DrawStep(i)) break; // 목록이 바뀌었으면 이번에는 여기까지만 그린다
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("+ 단계 추가", GUILayout.Height(24)))
        {
            ShowAddMenu();
        }

        serializedObject.ApplyModifiedProperties();
    }

    // 단계 하나를 그린다. 순서 변경/삭제로 목록이 바뀌었으면 true
    private bool DrawStep(int index)
    {
        SerializedProperty element = _steps.GetArrayElementAtIndex(index);
        object value = element.managedReferenceValue;

        string title = $"{index + 1}. {GetMenuName(value?.GetType())}";
        string summary = (value as CutsceneStep)?.Summary;
        if (!string.IsNullOrEmpty(summary)) title += $"  ({summary})";

        bool changed = false;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.BeginHorizontal();

        element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, title, true);

        using (new EditorGUI.DisabledScope(index == 0))
        {
            if (GUILayout.Button("▲", GUILayout.Width(24)))
            {
                _steps.MoveArrayElement(index, index - 1);
                changed = true;
            }
        }
        using (new EditorGUI.DisabledScope(index == _steps.arraySize - 1))
        {
            if (!changed && GUILayout.Button("▼", GUILayout.Width(24)))
            {
                _steps.MoveArrayElement(index, index + 1);
                changed = true;
            }
        }
        if (!changed && GUILayout.Button("X", GUILayout.Width(24)))
        {
            _steps.DeleteArrayElementAtIndex(index);
            changed = true;
        }

        EditorGUILayout.EndHorizontal();

        if (!changed && element.isExpanded)
        {
            if (value == null)
            {
                EditorGUILayout.HelpBox("비어 있는 단계입니다. 단계 스크립트의 이름이 바뀌었거나 삭제되었을 수 있어요.", MessageType.Warning);
            }
            else
            {
                EditorGUI.indentLevel++;
                DrawChildren(element);
                EditorGUI.indentLevel--;
            }
        }

        EditorGUILayout.EndVertical();
        return changed;
    }

    // 단계 안의 필드들을 그린다
    private static void DrawChildren(SerializedProperty element)
    {
        SerializedProperty child = element.Copy();
        SerializedProperty end = element.GetEndProperty();

        if (!child.NextVisible(true)) return;

        while (!SerializedProperty.EqualContents(child, end))
        {
            EditorGUILayout.PropertyField(child, true);
            if (!child.NextVisible(false)) break;
        }
    }

    private void ShowAddMenu()
    {
        GenericMenu menu = new GenericMenu();

        var types = TypeCache.GetTypesDerivedFrom<CutsceneStep>()
            .Where(t => !t.IsAbstract && !t.IsGenericType)
            .OrderBy(GetMenuName);

        foreach (Type type in types)
        {
            Type captured = type;
            menu.AddItem(new GUIContent(GetMenuName(type)), false, () => AddStep(captured));
        }

        menu.ShowAsContext();
    }

    private void AddStep(Type type)
    {
        serializedObject.Update();

        int index = _steps.arraySize;
        _steps.arraySize++;

        SerializedProperty element = _steps.GetArrayElementAtIndex(index);
        element.managedReferenceValue = Activator.CreateInstance(type);
        element.isExpanded = true;

        serializedObject.ApplyModifiedProperties();
    }

    private static string GetMenuName(Type type)
    {
        if (type == null) return "(비어 있음)";

        var attribute = (CutsceneStepMenuAttribute)Attribute.GetCustomAttribute(type, typeof(CutsceneStepMenuAttribute));
        return attribute != null ? attribute.MenuName : type.Name;
    }
}
