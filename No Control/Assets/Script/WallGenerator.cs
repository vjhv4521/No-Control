using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class WallGenerator : MonoBehaviour
{
   /// <summary>
/// 墙体生成器的静态类。
/// </summary>

    /// <summary>
    /// 创建墙体的方法。
    /// </summary>
    /// <param name="floorPositions">地板位置的集合</param>
    /// <param name="tilemapVisualizer">瓦片可视化器</param>
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList);
        // 在每个墙体位置上绘制基本墙体
        foreach (var position in basicWallPositions)
        {
            tilemapVisualizer.PaintSingleBasicWall(position);
        }
    }
    /// <summary>
    /// 在指定方向上查找墙体的方法。
    /// </summary>
    /// <param name="floorPositions">地板位置的集合</param>
    /// <param name="directionList">方向列表</param>
    /// <returns>墙体位置的集合</returns>
    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                // 如果邻居位置不在地板位置集合中，则认为是墙体位置
                if (!floorPositions.Contains(neighbourPosition))
                {
                    wallPositions.Add(neighbourPosition);
                }
            }
        }
        return wallPositions;
    }
}

