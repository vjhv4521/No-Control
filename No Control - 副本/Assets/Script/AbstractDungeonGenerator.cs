using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField, Header("瓦片可视化器")]
    protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField, Header("地牢生成的起始位置")]
    protected Vector2Int startPosition = Vector2Int.zero;
    /// <summary>
    /// 生成地牢的方法。
    /// </summary>
    public void GenerateDungeon()
    {
        tilemapVisualizer.Clear(); // 清空瓦片可视化器
        RunProceduralGeneration(); // 执行程序化生成
    }
    /// <summary>
    /// 执行程序化生成地牢的抽象方法，需要在子类中实现具体逻辑。
    /// </summary>
    protected abstract void RunProceduralGeneration();
}

