using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
/// <summary>
/// 自定义编辑器类，用于 RandomDungeonGenerator。
/// </summary>
[CustomEditor(typeof(AbstractDungeonGenerator), true)]
public class RandomDungeonGeneratorEditor : Editor
{
    private AbstractDungeonGenerator generator; // 地牢生成器对象
    private void Awake()
    {
        generator = (AbstractDungeonGenerator)target; // 获取目标对象并转换为地牢生成器类型
    }
    /// <summary>
    /// 在 Inspector 窗口中绘制自定义的 GUI。
    /// </summary>
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 绘制基类的默认 Inspector 界面
        if (GUILayout.Button("Create Dungeon")) // 创建地牢的按钮
        {
            generator.GenerateDungeon(); // 调用地牢生成器的方法生成地牢
        }
    }
}

