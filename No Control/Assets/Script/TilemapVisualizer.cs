using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField] 
    private Tilemap flootTilemap;
    [SerializeField] 
    private TileBase floorTile;
    [SerializeField] 
    private TileBase walltop;
    [SerializeField]
    private Tilemap wallTilemap;

    internal void PaintSingleBasicWall(Vector2Int position)
    {
        PaintSingleTile(wallTilemap, position, walltop);
    }

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(flootTilemap, floorPositions, floorTile);
    }

    private void PaintTiles(Tilemap tilemap, IEnumerable<Vector2Int> positions, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, position, tile);
        }
    }

    private void PaintSingleTile(Tilemap tilemap, Vector2Int position, TileBase tile)
    {
       var tilePosition = tilemap.WorldToCell((Vector3Int)position);
       tilemap.SetTile(tilePosition, tile);
    }

    public void Clear()
    {
        flootTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }
}