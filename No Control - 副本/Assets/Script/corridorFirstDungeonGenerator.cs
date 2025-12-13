using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Linq;
using System;
public class CorridorFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{
    [SerializeField]
    private int corridorLength = 14,corridorCount = 5;
    [SerializeField]
    [Range(0.1f,1)]
    private float roomPercent = 0.8f;
   protected override void RunProceduralGeneration()
   {
       CorridorFirstDungeonGeneration();
   }


private void CorridorFirstDungeonGeneration()
{
   HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();//存储地板位置的哈希表
   HashSet<Vector2Int> potentialRoomPositions = new HashSet<Vector2Int>();
   CreateCorridors(floorPositions, potentialRoomPositions);
   HashSet<Vector2Int> roomPositions = CreateRooms(potentialRoomPositions);//存储房间位置的哈希表

   List<Vector2Int> deadEnds = FindAllDeadEnds(floorPositions);//查找所有死胡同位置

   CreateRoomsAtDeadEnds(deadEnds, roomPositions);//在死胡同位置创建房间

   floorPositions.UnionWith(roomPositions);//将房间位置加入总地板位置集合

   tilemapVisualizer.PaintFloorTiles(floorPositions);//绘制地板
   WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);//生成墙体
}

private void CreateRoomsAtDeadEnds(List<Vector2Int> deadEnds, HashSet<Vector2Int> roomPositions)
{
    foreach(var position in deadEnds)//遍历所有死胡同位置
    {
        if(roomPositions.Contains(position))//如果当前死胡同位置已经是房间位置
        {
            continue;//跳过当前循环
        }
        var roomFloor = RunRandomWalk(randomWalkParameters, position);//调用随机游走算法，获取当前死胡同位置的房间地板位置
        roomPositions.UnionWith(roomFloor);//将当前死胡同位置的房间地板位置加入房间位置集合
    }
}
private List<Vector2Int> FindAllDeadEnds(HashSet<Vector2Int> floorPositions)
{
    List<Vector2Int> deadEnds = new List<Vector2Int>();//存储死胡同位置的列表
    foreach(var position in floorPositions)//遍历所有地板位置
    {
        
            int neighboursCount = 0;//邻居数量
            foreach(var direction in Direction2D.cardinalDirectionsList)//遍历所有方向
            {
                if(floorPositions.Contains(position + direction))//如果当前方向的邻居位置是地板位置
                {
                    neighboursCount++;//邻居数量加一
                }
            }
            if(neighboursCount == 1)//如果当前位置只有一个邻居
            {
                deadEnds.Add(position);//将当前位置加入死胡同位置列表
            }
        
    }
    return deadEnds;//返回死胡同位置列表
}

private HashSet<Vector2Int> CreateRooms(HashSet<Vector2Int> potentialRoomPositions)
{
    HashSet<Vector2Int> roomPositions = new HashSet<Vector2Int>();//存储房间位置的哈希表
    int roomToCreateCount = Mathf.RoundToInt(potentialRoomPositions.Count * roomPercent);//计算需要创建的房间数量
    List<Vector2Int> roomToCreate = potentialRoomPositions.OrderBy(x => Guid.NewGuid()).Take(roomToCreateCount).ToList();//将潜在房间位置集合转换为列表并随机排序
    foreach(var roomPosition in roomToCreate)//遍历需要创建的房间位置列表
    {
        var roomFloor = RunRandomWalk(randomWalkParameters, roomPosition);//调用随机游走算法，获取当前房间的地板位置
        roomPositions.UnionWith(roomFloor);//将当前房间的地板位置加入房间位置集合
    }
    return roomPositions;//返回房间位置集合
}

private void CreateCorridors(HashSet<Vector2Int> floorPositions, HashSet<Vector2Int> potentialRoomPositions)
    {
        var currentPosition = startPosition;//当前游走位置
        potentialRoomPositions.Add(currentPosition);//将起始位置加入潜在房间位置集合

        for(int i = 0;i<corridorCount;i++)//进行多次游走
        {
            // 调用随机游走工具类，获取本次迭代的路径（可能包含重复）
            var corridor = ProceduralGenerationAlgorithms.RandomWalkCorridor(currentPosition, corridorLength);//调用随机游走算法
            // 将单次游走的路径合并到总地板集合（UnionWith：添加所有不存在的元素）
            currentPosition = corridor[corridor.Count - 1];//将本次游走的最后一个位置作为下一次游走的起始位置
            potentialRoomPositions.Add(currentPosition);//将本次游走的最后一个位置加入潜在房间位置集合 

            floorPositions.UnionWith(corridor);//将本次游走路径加入地板位置集合
        }
    }
}