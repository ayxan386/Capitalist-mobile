using System;
using System.Collections;
using GameControl;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : NetworkBehaviour
{
    [SerializeField] private RectTransform playerDisplay;
    [SerializeField] private PlayerInfoDisplayer infoDisplayPrefab;
    private PlayerInfoDisplayer infoDisplay;

    [SyncVar(hook = nameof(OnPositionChanged))]
    private int position;

    [SyncVar(hook = nameof(OnOwnedMoneyChanged))]
    private int ownedMoney = GlobalConstants.StartingMoney;

    [SyncVar(hook = nameof(OnDisplayNameChanged))]
    private string displayName;

    [SyncVar]
    public bool eventMove; 

    public int OwnedMoney => ownedMoney;
    public int Position => position;
    public string DisplayName => displayName;
    public RectTransform DisplayEnt { get; private set; }

    public Player NextPlayer { get; set; }

    private static TileVariant[] boardData;
    public static Action<int> PlayerOwnedMoneyChanged;
    public static Action<Player> OnPlayerSpawned;
    public static Action<Player> OnPlayerDespawned;


    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer)
        {
            boardData ??= TilePlacer.Instance.GenerateBoardData();
            var tileNames = new string[boardData.Length];
            for (int i = 0; i < boardData.Length; i++)
            {
                tileNames[i] = boardData[i].displayName;
            }
            RpcDisplayBoard(tileNames);
        }


        StartCoroutine(WaitForBoardThenPlace());
        if (isOwned)
        {
            PlayerOwnedMoneyChanged += UpdatePlayerOwnedMoney;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        OnPlayerDespawned?.Invoke(this);
        Destroy(DisplayEnt.gameObject);
        Destroy(infoDisplay.gameObject);
    }

    private IEnumerator WaitForBoardThenPlace()
    {
        yield return new WaitUntil(() => TilePlacer.IsInitializeComplete);

        var holder = PlayerManager.Instance.PlayersHolder;
        DisplayEnt = Instantiate(playerDisplay, holder);
        if (isOwned)
        {
            infoDisplay = PlayerManager.Instance.SelfPlayersDisplayer.GetComponent<PlayerInfoDisplayer>();
        }
        else
        {
            var otherPlayers = PlayerManager.Instance.OtherPlayersDisplayers;
            infoDisplay = Instantiate(infoDisplayPrefab, otherPlayers);
        }

        OnPositionChanged(0, 0);
        CmdUpdateOwnedMoney(GlobalConstants.StartingMoney);

        OnPlayerSpawned?.Invoke(this);
    }

    [ClientRpc]
    private void RpcDisplayBoard(string[] boardData)
    {
        TilePlacer.Instance.PlaceTiles(boardData);
    }

    private void OnPositionChanged(int old, int current)
    {
        if (isOwned && !eventMove && old > current)
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

    private void OnDisplayNameChanged(string old, string current)
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

    private void ServerSideOnlyUpdateMoney(int diff)
    {
        ownedMoney += diff;
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

    public bool CanBuyTile(TileData tileData)
    {
        return Position == tileData.position
               && ownedMoney >= tileData.baseTile.cost
               && PlayerManager.Instance.CurrentPlayer.netId == netId;
    }

    [Command]
    public void CmdBuyTile(int tilePosition)
    {
        var data = TilePlacer.Instance.GetTileAt(tilePosition);
        if (CanBuyTile(data))
        {
            ownedMoney -= data.baseTile.cost;
            TilePlacer.Instance.RpcBoughtTile(data.position, netId);
        }
    }

    [Command]
    public void CmdCheckPosition()
    {
        var tileData = TilePlacer.Instance.GetTileAt(Position);
        print($"Checking {DisplayName} position {Position} with tile {tileData.baseTile.displayName}");
        if (!tileData.baseTile.government && tileData.isOwned && tileData.ownerId != netId)
        {
            var otherPlayer = PlayerManager.Instance.GetPlayerWithId(tileData.ownerId);
            var fee = tileData.baseTile.fee;
            otherPlayer.ServerSideOnlyUpdateMoney(fee);
            ownedMoney -= fee;
        }
        else
        {
            print("Inside else in check position");
            if (tileData.extraEvent != null)
            {
                RpcCheckTile();
            }
        }
    }

    [ClientRpc]
    private void RpcCheckTile()
    {
        print("Checking for extra events");
        if (!isOwned || !PlayerManager.Instance.CurrentPlayer.isOwned) return;

        var tileData = TilePlacer.Instance.GetTileAt(Position);
        tileData.extraEvent.PlayerArrived();
    }
}