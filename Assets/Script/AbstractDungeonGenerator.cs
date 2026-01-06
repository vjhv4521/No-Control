using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField, Header("瓦片可视化器")]
    protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField, Header("地牢生成的起始位置")]
    protected Vector2Int startPosition = Vector2Int.zero;
    [SerializeField]
    private bool useRandomSeed = true;
    [SerializeField]
    private GameObject wallBannerPrefab;

    protected HashSet<Vector2Int> floorPositionsCache = new HashSet<Vector2Int>();

    /// <summary>
    /// 生成地牢的方法。
    /// </summary>
    public void GenerateDungeon()
    {
        if (useRandomSeed) UnityEngine.Random.InitState(Environment.TickCount);
        tilemapVisualizer.Clear(); // 清空瓦片可视化器
        RunProceduralGeneration(); // 执行程序化生成
        PlaceBannerIfNeeded();
    }
    /// <summary>
    /// 执行程序化生成地牢的抽象方法，需要在子类中实现具体逻辑。
    /// </summary>
    protected abstract void RunProceduralGeneration();

    protected void SetFloor(HashSet<Vector2Int> floor)
    {
        floorPositionsCache = floor ?? new HashSet<Vector2Int>();
    }

    private void PlaceBannerIfNeeded()
    {
        if (wallBannerPrefab == null) return;
        if (floorPositionsCache == null || floorPositionsCache.Count == 0) return;

        Vector2Int origin = Vector2Int.zero;
        if (Player.Instance != null)
        {
            var p = Player.Instance.transform.position;
            origin = new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y));
        }
        else
        {
            origin = startPosition;
        }

        Vector2Int target = GetFarthestReachable(origin, floorPositionsCache);
        Vector3 spawnPos = tilemapVisualizer != null 
            ? tilemapVisualizer.GetCellCenterWorld(target) 
            : new Vector3(target.x, target.y, 0f);
        var banner = Instantiate(wallBannerPrefab, spawnPos, Quaternion.identity);
        var box = banner.GetComponent<BoxCollider2D>();
        if (box == null) box = banner.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        var trigger = banner.GetComponent<LevelEndTrigger>();
        if (trigger == null) trigger = banner.AddComponent<LevelEndTrigger>();
    }

    private Vector2Int GetFarthestReachable(Vector2Int start, HashSet<Vector2Int> floor)
    {
        if (!floor.Contains(start))
        {
            Vector2Int closest = start;
            int best = int.MaxValue;
            foreach (var pos in floor)
            {
                int d = Mathf.Abs(pos.x - start.x) + Mathf.Abs(pos.y - start.y);
                if (d < best) { best = d; closest = pos; }
            }
            start = closest;
        }

        var dirs = Direction2D.cardinalDirectionsList;
        var dist = new Dictionary<Vector2Int, int>();
        var q = new Queue<Vector2Int>();
        dist[start] = 0;
        q.Enqueue(start);
        Vector2Int far = start;
        int farDist = 0;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int baseDist = dist[cur];
            if (baseDist > farDist) { farDist = baseDist; far = cur; }
            for (int i = 0; i < dirs.Count; i++)
            {
                var nxt = cur + dirs[i];
                if (!floor.Contains(nxt)) continue;
                if (dist.ContainsKey(nxt)) continue;
                dist[nxt] = baseDist + 1;
                q.Enqueue(nxt);
            }
        }
        return far;
    }
}
