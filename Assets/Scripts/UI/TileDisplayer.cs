using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileDisplayer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform playerLocation;
    [SerializeField] private Image icon;
    [SerializeField] private Image iconBackground;
    [SerializeField] private TextMeshProUGUI displayName;

    [Header("Highlighting")] [SerializeField]
    private Image highlightFrame;

    [SerializeField] private Color dimColor;
    [SerializeField] private Color highLightColor;

    private TileData data;
    private bool isForSale;

    public static event Action<TileData> OnTileClick;

    public static event Action<TileData> OnForSaleTileClick;

    public void Display(TileData data, bool forSale = false)
    {
        this.data = data;
        isForSale = forSale;
        var baseTile = data.baseTile;
        icon.sprite = baseTile.icon;
        iconBackground.color = baseTile.spriteColor;
        displayName.text = baseTile.displayName;

        if (forSale)
        {
            TilePlacer.OnTileSold += OnTileSold;
        }
    }

    public void PlacePlayer(Player player)
    {
        player.DisplayEnt.SetParent(playerLocation);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isForSale)
        {
            OnForSaleTileClick?.Invoke(data);
        }
        else
        {
            OnTileClick?.Invoke(data);
        }
    }

    private void OnTileSold(int obj)
    {
        if (obj == data.ownerId)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        TilePlacer.OnTileSold -= OnTileSold;
    }

    public void Highlight()
    {
        highlightFrame.color = highLightColor;
    }

    public void CancelHighlight()
    {
        highlightFrame.color = dimColor;
    }
}