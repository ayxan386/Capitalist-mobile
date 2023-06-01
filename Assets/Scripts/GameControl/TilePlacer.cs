using System;
using System.Collections;
using System.Collections.Generic;
using GameControl.helpers;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class TilePlacer : NetworkBehaviour
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
    [SerializeField] private DetailedTileDisplayer detailedTileDisplayer;
    [SerializeField] private Transform extrasParent;
    [SerializeField] private TileVariantSelectionData[] selectionDatas;

    [Header("SFX")] [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip randomSelectionClip;

    private TileData[] tileDatas;
    private Dictionary<string, TileVariant> allTiles;

    public TileDisplayer[] Tiles { get; private set; }
    public static TilePlacer Instance { get; private set; }

    public static bool IsInitializeComplete { get; private set; }
    public static Action<int> OnTileSold;

    private void Awake()
    {
        Instance = this;
        allTiles = new Dictionary<string, TileVariant>();
        foreach (var selectionData in selectionDatas)
        {
            allTiles.Add(selectionData.tileVariant.displayName, selectionData.tileVariant);
        }

        foreach (var selectionData in cornerTiles)
        {
            if (!allTiles.ContainsKey(selectionData.displayName))
                allTiles.Add(selectionData.displayName, selectionData);
        }
    }

    public void PlaceTiles(string[] data)
    {
        if (Tiles != null) return;
        var totalCount = CalculateTotalCount();
        Tiles = new TileDisplayer[totalCount];

        tileDatas = new TileData[data.Length];

        for (var index = 0; index < data.Length; index++)
        {
            var tileVariant = allTiles[data[index]];
            tileDatas[index] = new TileData(tileVariant, index);
            print($"tile at {index} has {tileVariant.extraEvents}");
            if (tileVariant.extraEvents != null)
            {
                print("Creating extra event object");
                tileDatas[index].extraEvent =
                    Instantiate(tileVariant.extraEvents, extrasParent).GetComponent<BaseTile>();
            }
        }

        for (int i = 0; i < tileDatas.Length; i++)
        {
            var holder = FindHolder(i);
            Tiles[i] = Instantiate(FindPrefab(holder), holder);
            Tiles[i].Display(tileDatas[i]);
        }


        TileDisplayer.OnTileClick += TileDisplayerOnClick;
        Player.OnPlayerDespawned += OnPlayerDespawned;
        IsInitializeComplete = true;
        print("Board tiles placed");
    }

    private void OnPlayerDespawned(Player obj)
    {
        foreach (var tileData in tileDatas)
        {
            if (tileData.isOwned && tileData.ownerId == obj.netId)
            {
                tileData.isOwned = false;
                tileData.ownerId = 0;
            }
        }
    }

    private void TileDisplayerOnClick(TileData obj)
    {
        detailedTileDisplayer.Display(obj);
    }

    public void UpdateTileDetails(int position)
    {
        if (detailedTileDisplayer != null && tileDatas != null)
            detailedTileDisplayer.IfSameThenDisplay(tileDatas[position]);
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
        print("Generating board data ");
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
        GenerateTileVariantForSection(k, numberOfTiles.x, null, res);

        return res;
    }

    [ClientRpc]
    public void RpcBoughtTile(int dataPosition, uint ownerId)
    {
        var tileData = tileDatas[dataPosition];
        tileData.isOwned = true;
        tileData.ownerId = ownerId;
        detailedTileDisplayer.IfSameThenDisplay(tileData);
    }

    [ClientRpc]
    public void RpcAnimatedSelection(int selectedPos)
    {
        StartCoroutine(AnimateSelection(selectedPos));
    }

    [Command(requiresAuthority = false)]
    public void CmdUpgradedTile(int position, int newFee)
    {
        RpcUpgradedTile(position, newFee, tileDatas[position].CalculateTilePrice());
    }

    [ClientRpc]
    private void RpcUpgradedTile(int position, int newFee, int sellPrice)
    {
        tileDatas[position].fee = newFee;
        tileDatas[position].sellPrice = sellPrice;
    }

    private IEnumerator AnimateSelection(int selectedPos)
    {
        audioSource.PlayOneShot(randomSelectionClip);
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < Tiles.Length; j++)
            {
                Tiles[j].Highlight();
                yield return new WaitForSeconds(0.1f);
                Tiles[j].CancelHighlight();
            }
        }

        for (int i = 0; i < selectedPos; i++)
        {
            Tiles[i].Highlight();
            yield return new WaitForSeconds(0.1f);
            Tiles[i].CancelHighlight();
        }

        Tiles[selectedPos].Highlight();
        PlayerMovementHelper.Instance.CmdAnimationComplete();
        yield return new WaitForSeconds(1.2f);
        Tiles[selectedPos].CancelHighlight();
    }

    public TileData GetTileAt(int tilePosition)
    {
        return tileDatas[tilePosition];
    }

    public List<TileData> GetTilesOwnedBy(Player player)
    {
        return new List<TileData>(tileDatas).FindAll(tile => tile.isOwned && tile.ownerId == player.netId);
    }

    public List<TileData> GetTilesOwnedBy(uint player)
    {
        return new List<TileData>(tileDatas).FindAll(tile => tile.isOwned && tile.ownerId == player);
    }


    [ClientRpc]
    public void RpcSoldTile(int tilePosition)
    {
        var tileData = tileDatas[tilePosition];
        tileData.isOwned = false;
        detailedTileDisplayer.IfSameThenDisplay(tileData);
        OnTileSold?.Invoke(tilePosition);
    }
}


[Serializable]
public class TileVariantSelectionData
{
    public TileVariant tileVariant;

    public float chance;
}

public class TileData
{
    public Guid id;
    public TileVariant baseTile;
    public bool isOwned;
    public uint ownerId;
    public int position;
    public BaseTile extraEvent;
    public int fee;
    public int sellPrice;

    public TileData(TileVariant baseTile, int position)
    {
        this.baseTile = baseTile;
        this.position = position;
        fee = baseTile.fee ;
        id = Guid.NewGuid();
        isOwned = false;
        sellPrice = baseTile.cost;
    }

    public int CalculateTilePrice()
    {
        return (int)(((fee - baseTile.fee) * 10 + baseTile.cost) * (1 + Random.Range(-0.1f, 0.1f)));
    }
}