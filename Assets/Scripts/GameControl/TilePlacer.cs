using System;
using UnityEngine;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private TileDisplayer tileDisplayerPrefabHorizontal;
    [SerializeField] private TileDisplayer tileDisplayerPrefabVertical;
    [SerializeField] private Vector2Int numberOfTiles;

    [Header("Tile holders")] [SerializeField]
    private Transform leftColumn;

    [SerializeField] private Transform rightColumn;
    [SerializeField] private Transform bottomRow;
    [SerializeField] private Transform topRow;

    [ContextMenu("Place tiles")]
    public void PlaceTiles()
    {
        for (int i = 0; i < numberOfTiles.y; i++)
        {
            Instantiate(tileDisplayerPrefabVertical, leftColumn);
        }

        for (int i = 0; i < numberOfTiles.y; i++)
        {
            Instantiate(tileDisplayerPrefabVertical, rightColumn);
        }

        for (int i = 0; i < numberOfTiles.x; i++)
        {
            Instantiate(tileDisplayerPrefabHorizontal, bottomRow);
        }

        for (int i = 0; i < numberOfTiles.x; i++)
        {
            Instantiate(tileDisplayerPrefabHorizontal, topRow);
        }
    }
}


[Serializable]
public class TileData
{
    public Guid id;
    public Sprite icon;
    public string displayName;
}