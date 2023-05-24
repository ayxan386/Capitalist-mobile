using GameControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetailedTileDisplayer : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI displayName;
    [SerializeField] private TextMeshProUGUI ownerName;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TextMeshProUGUI purchaseButtonText;

    private TileData data;

    public void Display(TileData data)
    {
        this.data = data;
        var baseTile = data.baseTile;
        icon.sprite = baseTile.icon;
        icon.color = baseTile.spriteColor;
        displayName.text = "Name: " + baseTile.displayName;
        description.text = baseTile.description;

        if (!data.isOwned)
        {
            purchaseButton.gameObject.SetActive(true);
            ownerName.alpha = 0;
            purchaseButtonText.text = "$" + baseTile.cost;
            var player = PlayerManager.Instance.OwnedPlayer;
            purchaseButton.interactable = player.CanBuyTile(data);
        }
        else
        {
            ownerName.alpha = 1;
            ownerName.text = "Owner: " + PlayerManager.Instance.GetPlayerWithId(data.ownerId).DisplayName;
            purchaseButton.gameObject.SetActive(false);
        }
    }

    public void OnBuyTile()
    {
        if (data.isOwned) return;

        var player = PlayerManager.Instance.OwnedPlayer;
        player.CmdBuyTile(data.position);
    }

    public void IfSameThenDisplay(TileData passedData)
    {
        if (data.id == passedData.id) Display(passedData);
    }
}