using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileDisplayer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform playerLocation;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI displayName;

    private TileData data;

    public static event Action<TileData> OnTileClick;

    public void Display(TileData data)
    {
        this.data = data;
        var baseTile = data.baseTile;
        icon.sprite = baseTile.icon;
        icon.color = baseTile.spriteColor;
        displayName.text = baseTile.displayName;
    }

    public void PlacePlayer(Player player)
    {
        player.DisplayEnt.SetParent(playerLocation);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTileClick?.Invoke(data);
    }
}