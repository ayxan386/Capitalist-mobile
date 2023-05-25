using System;
using GameControl.helpers;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class TeleportationCorner : BaseTile
{
    private GameObject teleportationOptionsMenu;

    private void Start()
    {
        teleportationOptionsMenu = GameObject.Find("TeleportationOptionsMenu");
        teleportationOptionsMenu.transform.Find("RandomButton").GetComponent<Button>().onClick
            .AddListener(RandomTeleportation);
        teleportationOptionsMenu.transform.Find("SelectedTeleport").GetComponent<Button>().onClick
            .AddListener(SelectedTeleportation);
    }

    private void SelectedTeleportation()
    {
        teleportationOptionsMenu.transform.localScale = Vector3.zero;
    }

    private void RandomTeleportation()
    {
        teleportationOptionsMenu.transform.localScale = Vector3.zero;
        var tilesLength = TilePlacer.Instance.Tiles.Length;
        //TODO add selection animation
        var selectedTile = Random.Range(0, tilesLength);

        DiceRollHelper.Instance.CmdMoveToTile(selectedTile);
    }


    public override void PlayerArrived()
    {
        teleportationOptionsMenu.transform.localScale = Vector3.one;
        print("Player arrived");
    }
}