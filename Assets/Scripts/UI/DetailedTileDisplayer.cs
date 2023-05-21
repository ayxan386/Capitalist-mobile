using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DetailedTileDisplayer : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI displayName;
    [SerializeField] private TextMeshProUGUI ownerName;
    [SerializeField] private TextMeshProUGUI description;

    private TileData data;

    public void Display(TileData data)
    {
        this.data = data;
        var baseTile = data.baseTile;
        icon.sprite = baseTile.icon;
        icon.color = baseTile.spriteColor;
        displayName.text ="Name: " + baseTile.displayName;
        description.text = baseTile.description;
        ownerName.text = "Owner: " + (this.data.isOwned ? data.ownerId.ToString() : "NA");
    }
}