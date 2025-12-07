using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

// 简单随机游走地牢生成器：通过多次随机游走生成地牢地板位置
public class SimpleRandomWalkDungeonGenerator : MonoBehaviour
{
   [Header("随机游走参数")] // 分组序列化字段，提升Inspector可读性
   [SerializeField]
   protected Vector2Int startPosition = Vector2Int.zero;//初始化起始位置
   [SerializeField]
   private int iterations = 10;// 游走迭代次数：每次迭代生成一条随机路径
   [SerializeField]
   private int walkLength = 10;// 单次游走步数：每轮迭代走多少步
   [SerializeField]
   public bool startRandomlyEachIteration = true;//是否每次随机开始位置

  // 执行程序化生成的入口方法（可挂载到按钮/启动逻辑）
   public void RunProceduralGeneration()
   {
       // 调用随机游走核心逻辑，获取所有地板位置（唯一）
       HashSet<Vector2Int> floorPositions = RunRandomWalk();//存储地板位置的哈希表
        // 遍历并打印所有地板位置（调试用，实际项目可替换为生成网格/物体）
       foreach(var position in floorPositions)//遍历地板位置
       {
           Debug.Log(position);//输出地板位置
       }
      
       
   }
 // 核心逻辑：执行多次随机游走，合并所有路径为地板位置集合
    protected HashSet<Vector2Int> RunRandomWalk()
   {
      var currentPosition = startPosition;//当前游走位置
      HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();//存储地板位置的哈希表
      for(int i = 0;i<iterations;i++)//进行多次游走
      {
         // 调用随机游走工具类，获取本次迭代的路径（可能包含重复）
          var path = ProceduralGenerationAlgorithms.SimpleRandomWalk(currentPosition, walkLength);//调用随机游走算法
          // 将单次游走的路径合并到总地板集合（UnionWith：添加所有不存在的元素）
          floorPositions.UnionWith(path);//将本次游走路径加入地板位置集合

          if (startRandomlyEachIteration)//如果每次随机开始位置
          {
            // 从已有地板位置中随机选一个作为下一次游走的起始位置
          currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));//从已有地板位置中随机选择一个作为新的起始位置
          }
      }

      return floorPositions;//返回地板位置集合
   }
}
