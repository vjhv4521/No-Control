using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class ProceduralGenerationAlgorithms
{
    // 随机游走算法：返回游走过程中访问过的所有唯一位置
    // 参数：startPosition 起始位置；walklength 游走步数
    public static List<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLength)
    {
        // 用List记录「行走路径的顺序」（游走算法通常需要保留步数顺序）
        List<Vector2Int> path = new List<Vector2Int>();
        // 可选：用HashSet辅助记录「是否访问过该位置」（去重）
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

        Vector2Int currentPosition = startPosition;
        path.Add(currentPosition);
        visitedPositions.Add(currentPosition);

        for (int i = 0; i < walkLength; i++)
        {
            // 随机四向移动
            Vector2Int direction = Direction2D.GetRandomCardinalDirection();
            currentPosition += direction;

            // 游走路径保留所有步数（哪怕重复走到同一位置）
            path.Add(currentPosition);
            // 同时记录「访问过的唯一位置」（可选，按需使用）
            visitedPositions.Add(currentPosition);
        }

        // 返回路径列表（主脚本需要这个类型）
        return path;
    }

    public static List<Vector2Int> RandomWalkCorridor(Vector2Int startPosition, int corridorLength)
    {
        List<Vector2Int> corridor = new List<Vector2Int>();
        var direction = Direction2D.GetRandomCardinalDirection();
        var currentPosition = startPosition;
        corridor.Add(currentPosition);
        for(int i = 0;i<corridorLength;i++)
        {
            currentPosition += direction;//根据随机方向移动
            corridor.Add(currentPosition);//将当前位置加入走廊
        }

        return corridor;
    }

    public static List<BoundsInt> BinarySpacePartitioning(BoundsInt spaceToSplit,int minWidth,int minHeight)
    {
        Queue<BoundsInt> roomsQueue = new Queue<BoundsInt>();
        List<BoundsInt> roomsList = new List<BoundsInt>();
        roomsQueue.Enqueue(spaceToSplit);
        while(roomsQueue.Count > 0)
        {
            var room = roomsQueue.Dequeue();
            if(room.size.x >= minWidth && room.size.y >= minHeight)
            {
                if(Random.value < 0.5f)
                {
                   if(room.size.x >= minWidth * 2)
                    {
                        SplitVertically(room,minWidth,roomsQueue); 
                    }
                   else if(room.size.y >= minHeight * 2)
                    {
                       SplitHorizontally(room,minHeight,roomsQueue);
                    }
                    
                    else if(room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
                else
                {
                    if(room.size.y >= minHeight * 2)
                    {
                       SplitHorizontally(room,minHeight,roomsQueue);
                    }
                    else if(room.size.x >= minWidth * 2)
                    {
                        SplitVertically(room,minWidth,roomsQueue);
                    }
                    else if(room.size.x >= minWidth && room.size.y >= minHeight)
                    {
                        roomsList.Add(room);
                    }
                }
            }
        }
        return roomsList;
    }

   private static void SplitVertically(BoundsInt room,int minWidth, Queue<BoundsInt> roomsQueue)
 {
   var xSplit = Random.Range(1, room.size.x);
   BoundsInt room1 = new BoundsInt(room.min,new Vector3Int(xSplit,room.size.y,room.size.z));
   BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x + xSplit,room.min.y,room.min.z),new Vector3Int(room.size.x - xSplit,room.size.y,room.size.z));
   roomsQueue.Enqueue(room1);
   roomsQueue.Enqueue(room2);
 }
   private static void SplitHorizontally(BoundsInt room,int minHeight, Queue<BoundsInt> roomsQueue)
 {
   var ySplit = Random.Range(1, room.size.y);
   BoundsInt room1 = new BoundsInt(room.min,new Vector3Int(room.size.x,ySplit,room.size.z));
   BoundsInt room2 = new BoundsInt(new Vector3Int(room.min.x,room.min.y + ySplit,room.min.z),new Vector3Int(room.size.x,room.size.y - ySplit,room.size.z));
   roomsQueue.Enqueue(room1);
   roomsQueue.Enqueue(room2);
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