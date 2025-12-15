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
    public bool chaosOnStart = true;
    public bool chaosOnHurt = true;

    [Header("键位映射（可在Inspector编辑）")]
    public List<KeyMapping> keyMappings = new List<KeyMapping>();
    private Dictionary<string, Key> actionKeyMap = new Dictionary<string, Key>();

    private void Awake()
    {
        // 初始化默认键位
        if (keyMappings.Count == 0)
        {
            keyMappings.Add(new KeyMapping { actionName = "MoveUp", key = Key.W });
            keyMappings.Add(new KeyMapping { actionName = "MoveDown", key = Key.S });
            keyMappings.Add(new KeyMapping { actionName = "MoveLeft", key = Key.A });
            keyMappings.Add(new KeyMapping { actionName = "MoveRight", key = Key.D });
            keyMappings.Add(new KeyMapping { actionName = "Attack", key = Key.Space });
        }

        // 转换为字典
        foreach (var mapping in keyMappings)
        {
            actionKeyMap[mapping.actionName] = mapping.key;
        }

        if (chaosOnStart) ShuffleKeys();
    }

    public Key GetKeyForAction(string action)
    {
        if (actionKeyMap.TryGetValue(action, out var key)) return key;
        return Key.None;
    }

    public void OnPlayerHurt()
    {
        if (!chaosOnHurt || actionKeyMap.Count < 2) return;

        List<string> keys = new List<string>(actionKeyMap.Keys);
        int a = Random.Range(0, keys.Count);
        int b;
        do { b = Random.Range(0, keys.Count); } while (a == b);

        string actionA = keys[a];
        string actionB = keys[b];

        Key temp = actionKeyMap[actionA];
        actionKeyMap[actionA] = actionKeyMap[actionB];
        actionKeyMap[actionB] = temp;

        // 同步回可序列化列表（供Inspector查看）
        UpdateKeyMappings();
        Debug.Log($"[Chaos] 键位交换：{actionA} ({actionKeyMap[actionA]}) ⇄ {actionB} ({actionKeyMap[actionB]})");
    }

    private void ShuffleKeys()
    {
        List<string> keys = new List<string>(actionKeyMap.Keys);
        for (int i = keys.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string a = keys[i], b = keys[j];
            Key temp = actionKeyMap[a];
            actionKeyMap[a] = actionKeyMap[b];
            actionKeyMap[b] = temp;
        }
        UpdateKeyMappings();
        Debug.Log("[Chaos] 初始键位洗牌完成");
    }

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