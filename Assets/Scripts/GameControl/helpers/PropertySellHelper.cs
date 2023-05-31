using System;
using System.Collections.Generic;
using GameControl;
using Mirror;
using UnityEngine;

public class PropertySellHelper : NetworkBehaviour
{
    [SerializeField] private GameObject sellMenu;
    [SerializeField] private Transform propertyHolder;
    [SerializeField] private TileDisplayer propertyPrefab;
    [SerializeField] private PropertySellDetailes propertySellDetailes;
    public static PropertySellHelper Instance { get; private set; }

    public static Action<bool> OnPropertyAction;
    private int requiredAmount;

    private void Awake()
    {
        Instance = this;
    }

    [ClientRpc]
    public void RpcSellMyProperties(uint sellerId, int fee)
    {
        var seller = PlayerManager.Instance.GetPlayerWithId(sellerId);
        if (!seller.isOwned) return;

        requiredAmount = fee;
        var ownedTiles = TilePlacer.Instance.GetTilesOwnedBy(seller);
        if (ownedTiles.Count == 0)
        {
            OnPropertyAction?.Invoke(false);
            return;
        }

        sellMenu.transform.localScale = Vector3.one;
        UpdateUI(ownedTiles);

        propertySellDetailes.Display(ownedTiles[0]);
    }

    private void UpdateUI(List<TileData> ownedTiles)
    {
        for (int i = 0; i < propertyHolder.childCount; i++)
        {
            Destroy(propertyHolder.GetChild(i).gameObject);
        }


        foreach (var ownedTile in ownedTiles)
        {
            Instantiate(propertyPrefab, propertyHolder).Display(ownedTile, true);
        }
    }

    public void OnCancel()
    {
        OnPropertyAction?.Invoke(false);
    }

    public void PropertySellClicked(TileData data)
    {
        requiredAmount -= data.sellPrice;
        if (requiredAmount <= 0)
        {
            sellMenu.transform.localScale = Vector3.zero;
        }

        var player = PlayerManager.Instance.GetPlayerWithId(data.ownerId);
        player.CmdSellTile(data.position, requiredAmount);
    }
}