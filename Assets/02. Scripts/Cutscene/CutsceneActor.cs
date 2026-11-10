using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬 안의 오브젝트에 컷씬용 이름표(Id)를 붙인다.
/// 컷씬 에셋(CutsceneData)은 씬 오브젝트를 직접 참조할 수 없어서, 단계들은 이 Id로 오브젝트를 찾는다.
/// 예: NPC에 "No5", 컷씬 카메라에 "No5Cam"
/// </summary>
public class CutsceneActor : MonoBehaviour
{
    private static readonly Dictionary<string, CutsceneActor> _registry = new Dictionary<string, CutsceneActor>();

    [SerializeField] private string _id;

    public string Id => _id;

    private void Awake()
    {
        if (string.IsNullOrEmpty(_id))
        {
            Debug.LogWarning($"[CutsceneActor] Id가 비어 있습니다: {name}", this);
            return;
        }

        if (_registry.TryGetValue(_id, out CutsceneActor existing) && existing != null && existing != this)
        {
            Debug.LogWarning($"[CutsceneActor] Id가 중복되었습니다: {_id} ({existing.name}, {name})", this);
        }
        _registry[_id] = this;
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(_id) && _registry.TryGetValue(_id, out CutsceneActor registered) && registered == this)
        {
            _registry.Remove(_id);
        }
    }

    /// <summary>
    /// Id로 액터를 찾는다. 못 찾으면 경고를 남기고 false.
    /// </summary>
    public static bool TryFind(string id, out CutsceneActor actor)
    {
        actor = null;
        if (string.IsNullOrEmpty(id)) return false;

        if (_registry.TryGetValue(id, out actor) && actor != null) return true;

        Debug.LogWarning($"[CutsceneActor] '{id}' 액터를 찾지 못했습니다. 씬에 CutsceneActor가 있고 켜져 있는지 확인하세요.");
        actor = null;
        return false;
    }

    /// <summary>
    /// Id로 액터를 찾아 그 오브젝트(또는 자식)의 컴포넌트를 돌려준다. 못 찾으면 null.
    /// </summary>
    public static T FindComponent<T>(string id) where T : Component
    {
        if (!TryFind(id, out CutsceneActor actor)) return null;

        T component = actor.GetComponentInChildren<T>(true);
        if (component == null)
        {
            Debug.LogWarning($"[CutsceneActor] '{id}' 액터에 {typeof(T).Name} 컴포넌트가 없습니다.", actor);
        }
        return component;
    }
}
