using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEngine.Tilemaps;

// 简单随机游走地牢生成器：通过多次随机游走生成地牢地板位置
public class SimpleRandomWalkDungeonGenerator : AbstractDungeonGenerator
{
   [SerializeField]
   protected SimpleRandomWalkSO randomWalkParameters;

   //[SerializeField]
   //private TilemapVisualizer tilemapVisualizer;

  // 执行程序化生成的入口方法（可挂载到按钮/启动逻辑）
   protected override void RunProceduralGeneration()
   {
       // 调用随机游走核心逻辑，获取所有地板位置（唯一）
       HashSet<Vector2Int> floorPositions = RunRandomWalk(randomWalkParameters, startPosition);//存储地板位置的哈希表
       tilemapVisualizer.PaintFloorTiles(floorPositions);//绘制地板
       WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);//生成墙体
   }

   public void Clear()
    {
        tilemapVisualizer.Clear(); // 调用可视化器的清空方法，而非直接操作Tilemap
    }
 // 核心逻辑：执行多次随机游走，合并所有路径为地板位置集合
    protected HashSet<Vector2Int> RunRandomWalk(SimpleRandomWalkSO parameters, Vector2Int position)
   {
      var currentPosition = position;//当前游走位置
      HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();//存储地板位置的哈希表
      for(int i = 0;i<parameters.iterations;i++)//进行多次游走
      {
         // 调用随机游走工具类，获取本次迭代的路径（可能包含重复）
          var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, parameters.walkLength);//调用随机游走算法
          // 将单次游走的路径合并到总地板集合（UnionWith：添加所有不存在的元素）
          floorPositions.UnionWith(path);//将本次游走路径加入地板位置集合

          if (parameters.startRandomlyEachIteration)//如果每次随机开始位置
          {
            // 从已有地板位置中随机选一个作为下一次游走的起始位置
          currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));//从已有地板位置中随机选择一个作为新的起始位置
          }
      }

      return floorPositions;//返回地板位置集合
   }
    
}
