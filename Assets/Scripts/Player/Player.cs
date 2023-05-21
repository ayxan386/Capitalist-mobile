using System;
using System.Collections;
using GameControl;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private RectTransform playerDisplay;
    [SerializeField] private PlayerInfoDisplayer infoDisplayPrefab;
    private PlayerInfoDisplayer infoDisplay;

    [SyncVar(hook = nameof(OnPositionChanged))]
    private int position;

    [SyncVar(hook = nameof(OnOwnedMoneyChanged))]
    private int ownedMoney = GlobalConstants.StartingMoney;

    [SyncVar] private string displayName;

    public int OwnedMoney => ownedMoney;
    public int Position => position;
    public string DisplayName => displayName;
    public RectTransform DisplayEnt { get; private set; }
    public Player NextPlayer { get; set; }

    private static TileVariant[] boardData;
    public static Action<int> PlayerOwnedMoneyChanged;
    public static Action<Player> OnPlayerSpawned;


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer)
        {
            boardData ??= TilePlacer.Instance.GenerateBoardData();
            RpcDisplayBoard(boardData);
        }


        StartCoroutine(WaitForBoardThenPlace());
        if (isOwned)
        {
            PlayerOwnedMoneyChanged += UpdatePlayerOwnedMoney;
        }
    }

    private IEnumerator WaitForBoardThenPlace()
    {
        yield return new WaitUntil(() => TilePlacer.IsInitializeComplete);

        var holder = GameObject.Find("Players");
        DisplayEnt = Instantiate(playerDisplay, holder.transform);
        if (isOwned)
        {
            infoDisplay = GameObject.Find("PlayerSelfDisplay").GetComponent<PlayerInfoDisplayer>();
        }
        else
        {
            var otherPlayers = GameObject.Find("OtherPlayers");
            infoDisplay = Instantiate(infoDisplayPrefab, otherPlayers.transform);
        }

        OnPositionChanged(0, 0);
        // CmdUpdatePosition(0);
        CmdUpdateOwnedMoney(GlobalConstants.StartingMoney);

        OnPlayerSpawned?.Invoke(this);
    }

    [ClientRpc]
    private void RpcDisplayBoard(TileVariant[] boardData)
    {
        TilePlacer.Instance.PlaceTiles(boardData);
    }

    private void OnPositionChanged(int old, int current)
    {
        if (isOwned && old > current)
        {
            PlayerOwnedMoneyChanged?.Invoke(GlobalConstants.RoundSalary);
        }

        print($"Placing player: {DisplayName} with id {netId} at position {current}");
        UpdateInfo();
    }

    private void OnOwnedMoneyChanged(int old, int current)
    {
        UpdateInfo();
    }

    [Command]
    public void CmdUpdatePosition(int newPos)
    {
        position = newPos;
    }

    [Command]
    public void CmdUpdateOwnedMoney(int amount)
    {
        ownedMoney = amount;
    }

    public void CmdUpdateDisplayName(string newName)
    {
        displayName = newName;
    }

    public void UpdateInfo()
    {
        infoDisplay.Display(this);
        TilePlacer.Instance.Tiles[Position].PlacePlayer(this);
    }

    private void UpdatePlayerOwnedMoney(int difference)
    {
        CmdUpdateOwnedMoney(OwnedMoney + difference);
    }
}