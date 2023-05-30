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
    private GameObject teleportationConfirmationMenu;
    private TileData lastClickedTile;
    private Button confirmButton;

    private void Start()
    {
        teleportationOptionsMenu = GameObject.Find("TeleportationOptionsMenu");
        randomButton = teleportationOptionsMenu.transform.Find("RandomButton").GetComponent<Button>();
        randomButton.onClick.AddListener(RandomTeleportation);
        selectionButton = teleportationOptionsMenu.transform.Find("SelectedTeleport").GetComponent<Button>();
        selectionButton.onClick.AddListener(SelectedTeleportation);
        teleportationConfirmationMenu = GameObject.Find("TeleportationConfirmation");
        confirmButton = teleportationConfirmationMenu.transform.Find("Confirm").GetComponent<Button>();
        confirmButton.interactable = false;
        confirmButton.onClick.AddListener(ConfirmTeleportation);
    }

    private void ConfirmTeleportation()
    {
        confirmButton.interactable = false;
        TilePlacer.Instance.Tiles[lastClickedTile.position].CancelHighlight();
        PlayerMovementHelper.Instance.CmdDirectMoveToTile(lastClickedTile.position);
        teleportationConfirmationMenu.transform.localScale = Vector3.zero;
        lastClickedTile = null;
        TileDisplayer.OnTileClick -= OnTileClick;
    }

    private void SelectedTeleportation()
    {
        teleportationOptionsMenu.transform.localScale = Vector3.zero;
        teleportationConfirmationMenu.transform.localScale = Vector3.one;
        ownedPlayer.CmdUpdateOwnedMoney(ownedPlayer.OwnedMoney - selectedCost);
        TileDisplayer.OnTileClick += OnTileClick;
    }

    private void OnTileClick(TileData obj)
    {
        if (lastClickedTile != null)
        {
            TilePlacer.Instance.Tiles[lastClickedTile.position].CancelHighlight();
        }

        TilePlacer.Instance.Tiles[obj.position].Highlight();
        confirmButton.interactable = true;
        lastClickedTile = obj;
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


    public override void PlayerArrived(Player player, TileData selfData)
    {
        ownedPlayer = player;
        randomButton.interactable = player.OwnedMoney >= randomCost;
        selectionButton.interactable = player.OwnedMoney >= selectedCost;

        if (randomButton.interactable || selectionButton.interactable)
            teleportationOptionsMenu.transform.localScale = Vector3.one;

        print("Player arrived at teleportation corner");
    }
}