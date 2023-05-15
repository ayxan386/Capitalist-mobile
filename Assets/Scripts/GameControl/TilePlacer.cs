using System;
using UnityEngine;
using Random = UnityEngine.Random;

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

    [SerializeField] private TileVariantSelectionData[] selectionDatas;

    public TileDisplayer[] Tiles { get; private set; }
    public static TilePlacer Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [ContextMenu("Place tiles")]
    public void PlaceTiles()
    {
        NormalizeTileChances();
        Tiles = new TileDisplayer[numberOfTiles.y * 2 + numberOfTiles.x * 2];
        var k = 0;

        k = GenerateTileForSection(k, numberOfTiles.y, tileDisplayerPrefabVertical, leftColumn);
        k = GenerateTileForSection(k, numberOfTiles.x, tileDisplayerPrefabHorizontal, topRow);
        k = GenerateTileForSection(k, numberOfTiles.y, tileDisplayerPrefabVertical, rightColumn);
        k = GenerateTileForSection(k, numberOfTiles.x, tileDisplayerPrefabHorizontal, bottomRow);
    }

    private TileVariant SelectRandomTileVariant()
    {
        var roll = Random.value;
        int i = 0;
        while (true)
        {
            roll -= selectionDatas[i].chance;
            if (roll >= 0 || i > 100) i++;
            else break;

            i %= selectionDatas.Length;
        }

        return selectionDatas[i].tileVariant;
    }

    private void NormalizeTileChances()
    {
        var sum = 0f;
        foreach (var data in selectionDatas)
        {
            sum += data.chance;
        }

        foreach (var data in selectionDatas)
        {
            data.chance /= sum;
        }
    }


    private int GenerateTileForSection(int k, int count, TileDisplayer prefab, Transform holder)
    {
        for (int i = 0; i < count; i++)
        {
            var tileDisplayer = Instantiate(prefab, holder);
            Tiles[k++] = tileDisplayer;
            tileDisplayer
                .Display(SelectRandomTileVariant());
        }

        return k;
    }
}


[Serializable]
public class TileVariantSelectionData
{
    public TileVariant tileVariant;

    public float chance;
    // public bool used;
}