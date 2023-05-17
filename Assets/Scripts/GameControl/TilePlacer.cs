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
    [SerializeField] private Transform[] corners;
    [SerializeField] private TileVariant[] cornerTiles;

    [SerializeField] private TileVariantSelectionData[] selectionDatas;

    public TileDisplayer[] Tiles { get; private set; }
    public static TilePlacer Instance { get; private set; }

    public static Action<bool> OnTileInitializeComplete;

    private void Awake()
    {
        Instance = this;
    }

    [ContextMenu("Place tiles")]
    public void PlaceTiles()
    {
        NormalizeTileChances();
        Tiles = new TileDisplayer[numberOfTiles.y * 2 + numberOfTiles.x * 2 + 4];
        var k = 0;

        k = GenerateTileForSection(k, 1, tileDisplayerPrefabVertical, corners[0], cornerTiles[0]);
        k = GenerateTileForSection(k, numberOfTiles.y, tileDisplayerPrefabVertical, leftColumn, null);
        k = GenerateTileForSection(k, 1, tileDisplayerPrefabVertical, corners[1], cornerTiles[1]);
        k = GenerateTileForSection(k, numberOfTiles.x, tileDisplayerPrefabHorizontal, topRow, null);
        k = GenerateTileForSection(k, 1, tileDisplayerPrefabVertical, corners[2], cornerTiles[2]);
        k = GenerateTileForSection(k, numberOfTiles.y, tileDisplayerPrefabVertical, rightColumn, null);
        k = GenerateTileForSection(k, 1, tileDisplayerPrefabVertical, corners[3], cornerTiles[3]);
        GenerateTileForSection(k, numberOfTiles.x, tileDisplayerPrefabHorizontal, bottomRow, null);
        
        OnTileInitializeComplete?.Invoke(true);
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


    private int GenerateTileForSection(int k, int count, TileDisplayer prefab, Transform holder,
        TileVariant tileVariant)
    {
        for (int i = 0; i < count; i++)
        {
            var tileDisplayer = Instantiate(prefab, holder);
            Tiles[k++] = tileDisplayer;

            if (tileVariant == null)
            {
                tileDisplayer.Display(SelectRandomTileVariant());
            }
            else
            {
                tileDisplayer.Display(tileVariant);
            }
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