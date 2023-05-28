using System;
using GameControl.helpers;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TeleportationCorner : BaseTile
{
    [SerializeField] private int randomCost;
    [SerializeField] private int selectedCost;
    private GameObject teleportationOptionsMenu;
    private Button randomButton;
    private Button selectionButton;
    private Player ownedPlayer;

    private void Start()
    {
        teleportationOptionsMenu = GameObject.Find("TeleportationOptionsMenu");
        randomButton = teleportationOptionsMenu.transform.Find("RandomButton").GetComponent<Button>();
        randomButton.onClick
            .AddListener(RandomTeleportation);
        selectionButton = teleportationOptionsMenu.transform.Find("SelectedTeleport").GetComponent<Button>();
        selectionButton.onClick.AddListener(SelectedTeleportation);
    }

    private void SelectedTeleportation()
    {
        teleportationOptionsMenu.transform.localScale = Vector3.zero;
        ownedPlayer.CmdUpdateOwnedMoney(ownedPlayer.OwnedMoney - selectedCost);
        TileDisplayer.OnTileClick += OnTileClick;
    }

    private void OnTileClick(TileData obj)
    {
        PlayerMovementHelper.Instance.CmdDirectMoveToTile(obj.position);
        TileDisplayer.OnTileClick -= OnTileClick;
    }

    private void RandomTeleportation()
    {
        teleportationOptionsMenu.transform.localScale = Vector3.zero;
        ownedPlayer.CmdUpdateOwnedMoney(ownedPlayer.OwnedMoney - randomCost);
        PlayerMovementHelper.Instance.CmdSelectRandomTile();
        PlayerMovementHelper.OnSelectionComplete += OnSelectionComplete;
    }

    private void OnSelectionComplete(int selectedTile)
    {
        PlayerMovementHelper.Instance.CmdDirectMoveToTile(selectedTile);
        PlayerMovementHelper.OnSelectionComplete -= OnSelectionComplete;
    }


    public override void PlayerArrived(Player player)
    {
        ownedPlayer = player;
        randomButton.interactable = player.OwnedMoney >= randomCost;
        selectionButton.interactable = player.OwnedMoney >= selectedCost;

        if (randomButton.interactable || selectionButton.interactable)
            teleportationOptionsMenu.transform.localScale = Vector3.one;

        print("Player arrived at teleportation corner");
    }
}