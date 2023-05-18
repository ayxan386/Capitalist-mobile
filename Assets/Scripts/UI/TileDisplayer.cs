using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileDisplayer : MonoBehaviour
{
    [SerializeField] private RectTransform playerLocation;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI displayName;

    public void Display(TileVariant data)
    {
        icon.sprite = data.icon;
        icon.color = data.spriteColor;
        displayName.text = data.displayName;
    }

    public void PlacePlayer(Player player)
    {
        player.DisplayEnt.position = playerLocation.position;
    }
}