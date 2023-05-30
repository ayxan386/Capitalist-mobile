using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeableTile : BaseTile
{
    [SerializeField] private List<UpgradeData> upgrades;

    private int currentLevel = 0;
    private GameObject menu;
    private Button upgradeButton;
    private Button cancelButton;
    private TextMeshProUGUI upgradeText;

    private Player lastPlayer;
    private TileData selfData;
    private TextMeshProUGUI effectDescription;

    private void Start()
    {
        menu = GameObject.Find("UpgradeOptionsMenu");
        upgradeButton = menu.transform.Find("Upgrade").GetComponent<Button>();
        upgradeText = upgradeButton.transform.GetComponentInChildren<TextMeshProUGUI>();
        cancelButton = menu.transform.Find("Cancel").GetComponent<Button>();
        effectDescription = menu.transform.Find("EffectDescription").GetComponent<TextMeshProUGUI>();
    }

    private void Cancel()
    {
        menu.transform.localScale = Vector3.zero;
    }

    private void Upgrade()
    {
        print("Last player id: " + lastPlayer.netId);
        print("Upgrade size: " + upgrades.Count);
        lastPlayer.CmdUpdateOwnedMoney(lastPlayer.OwnedMoney - upgrades[currentLevel].cost);
        TilePlacer.Instance.CmdUpgradedTile(selfData.position,
            (int)Math.Floor(selfData.fee * upgrades[currentLevel].improvementFactor));
        currentLevel++;
        Cancel();
    }

    public override void PlayerArrived(Player player, TileData selfData)
    {
        cancelButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(Cancel);
        upgradeButton.onClick.AddListener(Upgrade);
        if (selfData.isOwned && selfData.ownerId == player.netId && currentLevel < upgrades.Count)
        {
            lastPlayer = player;
            this.selfData = selfData;
            menu.transform.localScale = Vector3.one;
            var cost = upgrades[currentLevel].cost;
            upgradeButton.interactable = player.OwnedMoney >= cost;
            upgradeText.text = $"${cost}";
            effectDescription.text =
                $"Fee: {this.selfData.fee} -> {(int)(selfData.fee * upgrades[currentLevel].improvementFactor)}";
        }
        else if (currentLevel == upgrades.Count)
        {
            menu.transform.localScale = Vector3.one;
            upgradeButton.interactable = false;
            upgradeText.text = "Max level";
            effectDescription.text = "Max level";
        }
    }
}


[Serializable]
public class UpgradeData
{
    public int cost;
    public float improvementFactor;
}