using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGenerationAlgorithms
{
    // 随机游走算法：返回游走过程中访问过的所有唯一位置
    // 参数：startPosition 起始位置；walklength 游走步数
    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walklength)//这里为什么要用哈希表，主要这是一个游走算法，需要记录每个点是否被访问过，而用哈希表会比较好处理
    {
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();//初始化哈希表
        
        path.Add(startPosition);//将起始位置加入路径

        var previousposition = startPosition;//记录上一个位置

        for(int i = 0;i<walklength;i++)
        {
            // 获取随机的四向方向（上/下/左/右），并计算新位置
            var newposition = previousposition + Direction2D.GetRandomCardinalDirection();//获取随机方向
            path.Add(newposition);//将新位置加入路径
            previousposition = newposition;//更新上一个位置
        }
        return path;//返回路径
    }
}
// 二维方向工具类：提供四向方向的常量和随机方向获取方法
public static class Direction2D
{
    // 四向方向列表（上、右、下、左）：static 保证全局唯一，readonly 防止被修改
    public static  List<Vector2Int> cardinalDirectionsList = new List<Vector2Int>
    {
        new Vector2Int(0, 1), // up
        new Vector2Int(1, 0), // right
        new Vector2Int(0, -1), // down
        new Vector2Int(-1, 0) // left
    };

    public static Vector2Int GetRandomCardinalDirection()
    {
        // Random.Range(0, count)：返回 [0, count) 范围内的整数（即 0/1/2/3）
        // 从方向列表中随机取一个方向返回
        return cardinalDirectionsList[Random.Range(0, cardinalDirectionsList.Count)];
    }
}