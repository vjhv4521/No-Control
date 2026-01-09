using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class KeyMapping // 可序列化的键位映射
{
    public string actionName;
    public Key key;
}

public class InputChaosManager : MonoBehaviour
{
    [Header("混乱设置")]
    public bool chaosOnStart = false;
    public bool chaosOnHurt = false;

    [Header("键位映射（可在Inspector编辑）")]
    public List<KeyMapping> keyMappings = new List<KeyMapping>();
    private Dictionary<string, Key> actionKeyMap = new Dictionary<string, Key>();

    private void Awake()
    {
        keyMappings.Clear();
        keyMappings.Add(new KeyMapping { actionName = "MoveUp", key = Key.W });
        keyMappings.Add(new KeyMapping { actionName = "MoveDown", key = Key.S });
        keyMappings.Add(new KeyMapping { actionName = "MoveLeft", key = Key.A });
        keyMappings.Add(new KeyMapping { actionName = "MoveRight", key = Key.D });
        keyMappings.Add(new KeyMapping { actionName = "Attack", key = Key.Space });

        // 转换为字典
        foreach (var mapping in keyMappings)
        {
            actionKeyMap[mapping.actionName] = mapping.key;
        }

        // 禁用初始洗牌
    }

    public Key GetKeyForAction(string action)
    {
        if (actionKeyMap.TryGetValue(action, out var key)) return key;
        return Key.None;
    }

    public void OnPlayerHurt()
    {
        // 禁用受伤打乱键位
        return;
    }

    private void ShuffleKeys() { }

    private void UpdateKeyMappings()
    {
        foreach (var mapping in keyMappings)
        {
            if (actionKeyMap.ContainsKey(mapping.actionName))
            {
                mapping.key = actionKeyMap[mapping.actionName];
            }
        }
    }
}
