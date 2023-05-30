using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PropertySellDetailes : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image iconBackground;
    [SerializeField] private TextMeshProUGUI displayName;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Button sellButton;
    [SerializeField] private TextMeshProUGUI sellButtonText;

    private TileData data;

    private void Start()
    {
        TileDisplayer.OnForSaleTileClick += OnForSaleTileClick;
    }

    private void OnForSaleTileClick(TileData obj)
    {
        Display(obj);
    }

    public void Display(TileData data)
    {
        this.data = data;
        var baseTile = data.baseTile;
        icon.sprite = baseTile.icon;
        iconBackground.color = baseTile.spriteColor;
        displayName.text = "Name: " + baseTile.displayName;
        description.text = baseTile.description + "\n Fee: " + data.fee;
        sellButtonText.text = "$" + data.CalculateTilePrice();
    }


    public void OnSell()
    {
        PropertySellHelper.Instance.PropertySellClicked(data);
    }
}