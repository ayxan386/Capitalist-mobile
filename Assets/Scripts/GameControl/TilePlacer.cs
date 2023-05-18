using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class TilePlacer : MonoBehaviour
{
    [SerializeField] private TileDisplayer tileDisplayerPrefabHorizontal;
    [SerializeField] private TileDisplayer tileDisplayerPrefabVertical;
    [SerializeField] private TileDisplayer tileDisplayerPrefabCorner;
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

    public static bool IsInitializeComplete { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void PlaceTiles(TileVariant[] data)
    {
        if (Tiles != null) return;
        ;
        var totalCount = CalculateTotalCount();
        Tiles = new TileDisplayer[totalCount];
        for (int i = 0; i < data.Length; i++)
        {
            var holder = FindHolder(i);
            Tiles[i] = Instantiate(FindPrefab(holder), holder);
            Tiles[i].Display(data[i]);
        }

        IsInitializeComplete = true;
        print("Board tiles placed");
    }

    private Transform FindHolder(int i)
    {
        if (i > 0 && i <= numberOfTiles.y)
        {
            return leftColumn;
        }

        if (i > numberOfTiles.y * 2 + 2 &&
            i <= numberOfTiles.y * 3 + 2)
        {
            return rightColumn;
        }

        if (i > numberOfTiles.y + 1 && i <= numberOfTiles.y * 2 + 1)
        {
            return topRow;
        }

        if (i > numberOfTiles.y * 3 + 3 && i < CalculateTotalCount())
        {
            return bottomRow;
        }

        return corners[i / (numberOfTiles.y + 1)];
    }

    private TileDisplayer FindPrefab(Transform i)
    {
        if (i == leftColumn || i == rightColumn) return tileDisplayerPrefabVertical;

        if (i == bottomRow || i == topRow) return tileDisplayerPrefabHorizontal;

        return tileDisplayerPrefabCorner;
    }

    private int CalculateTotalCount()
    {
        return numberOfTiles.x * 2 + numberOfTiles.y * 2 + 4;
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

    private int GenerateTileVariantForSection(int k, int count, TileVariant tileVariant, TileVariant[] res)
    {
        for (int i = 0; i < count; i++)
        {
            if (tileVariant == null)
            {
                res[k++] = SelectRandomTileVariant();
            }
            else
            {
                res[k++] = tileVariant;
            }
        }

        return k;
    }

    public int CalculatePosition(int pos, int dist)
    {
        return (pos + dist) % Tiles.Length;
    }

    public TileVariant[] GenerateBoardData()
    {
        NormalizeTileChances();
        var res = new TileVariant[CalculateTotalCount()];
        int k = 0;
        k = GenerateTileVariantForSection(k, 1, cornerTiles[0], res);
        k = GenerateTileVariantForSection(k, numberOfTiles.y, null, res);
        k = GenerateTileVariantForSection(k, 1, cornerTiles[1], res);
        k = GenerateTileVariantForSection(k, numberOfTiles.x, null, res);
        k = GenerateTileVariantForSection(k, 1, cornerTiles[2], res);
        k = GenerateTileVariantForSection(k, numberOfTiles.y, null, res);
        k = GenerateTileVariantForSection(k, 1, cornerTiles[3], res);
        k = GenerateTileVariantForSection(k, numberOfTiles.x, null, res);
        return res;
    }
}


[Serializable]
public class TileVariantSelectionData
{
    public TileVariant tileVariant;

    public float chance;
    // public bool used;
}