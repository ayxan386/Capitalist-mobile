using System;
using System.Collections;
using GameControl;
using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    [SerializeField] private RectTransform playerDisplay;
    [SerializeField] private PlayerInfoDisplayer infoDisplayPrefab;
    [SerializeField] private AudioClip moneyChangeSound;
    [SerializeField] private Sprite[] characterPics;

    private PlayerInfoDisplayer infoDisplay;

    [SyncVar(hook = nameof(OnPositionChanged))]
    private int position;

    [SyncVar(hook = nameof(OnOwnedMoneyChanged))]
    private int ownedMoney = GlobalConstants.StartingMoney;

    [SyncVar(hook = nameof(OnDisplayNameChanged))]
    private string displayName;

    [SyncVar] public bool eventMove;

    [SyncVar(hook = nameof(OnColorChanged))]
    private Color displayColor;

    [SyncVar(hook = nameof(OnIndexChanged))]
    private int pictureIndex;

    private bool canPay;

    public int OwnedMoney => ownedMoney;
    public int Position => position;
    public string DisplayName => displayName;
    public RectTransform DisplayEnt { get; private set; }

    public Player NextPlayer { get; set; }
    public Player PrevPlayer { get; set; }

    public Sprite ProfilePicture => characterPics[pictureIndex];

    public Color DisplayColor => displayColor;

    public int LastChange { get; set; }

    private static TileVariant[] boardData;
    public static Action<Player> OnPlayerSpawned;
    public static Action<Player> OnPlayerDespawned;


    public override void OnStartClient()
    {
        base.OnStartClient();
        print("Client started ");
        if (isServer)
        {
            print("Client is also server ");
            boardData = TilePlacer.Instance.GenerateBoardData();

            var tileNames = new string[boardData.Length];
            for (int i = 0; i < boardData.Length; i++)
            {
                tileNames[i] = boardData[i].displayName;
            }

            print("Telling client to display board");
            RpcDisplayBoard(tileNames);
        }


        StartCoroutine(WaitForBoardThenPlace());
        if (isOwned)
        {
            PropertySellHelper.OnPropertyAction += OnPropertyAction;
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

        if (isOwned)
        {
            ChooseRandomProfile();
            CmdUpdateOwnedMoney(GlobalConstants.StartingMoney);
        }

        OnPlayerSpawned?.Invoke(this);
        UpdateInfo();
    }

    private void ChooseRandomProfile()
    {
        var temp = Random.ColorHSV(0, 1, 0.8f, 1f, 0.6f, 1f);
        temp.a = 1;
        CmdUpdateColor(temp);
        CmdUpdateProfileIndex(Random.Range(0, characterPics.Length));
    }

    [ClientRpc]
    private void RpcDisplayBoard(string[] boardData)
    {
        print("Client side: placing board tiles");
        TilePlacer.Instance.PlaceTiles(boardData);
    }

    private void OnPositionChanged(int old, int current)
    {
        if (isOwned && !eventMove && old > current)
        {
            CmdCheckForCycle(old, current);
            DepositHelper.Instance.CmdCheckForDeposit(netId);
        }

        print($"Placing player: {DisplayName} with id {netId} at position {current}");
        UpdateInfo();
        TilePlacer.Instance?.UpdateTileDetails(current);
    }

    private void OnOwnedMoneyChanged(int old, int current)
    {
        LastChange += current - old;
        if (isOwned)
        {
            PlayerManager.Instance.SfxSource.PlayOneShot(moneyChangeSound);
        }

        UpdateInfo();
    }

    private void OnDisplayNameChanged(string old, string current)
    {
        PlayerManager.Instance.UpdateTurnIndicator();
        UpdateInfo();
    }

    public void UpdatePositionServerSide(int newPos, bool eventMovePass = false)
    {
        eventMove = eventMovePass;
        position = newPos;
    }

    [Command(requiresAuthority = false)]
    public void CmdUpdateOwnedMoney(int amount)
    {
        ownedMoney = amount;
    }
    
    [Command(requiresAuthority = false)]
    private void CmdCheckForCycle(int old, int current)
    {
        if (isOwned && !eventMove && old > current)
        {
            ownedMoney += GlobalConstants.RoundSalary;
        }
    }

    [Command]
    private void CmdUpdateProfileIndex(int index)
    {
        pictureIndex = index;
    }

    [Command]
    private void CmdUpdateColor(Color color)
    {
        displayColor = color;
    }

    private void OnColorChanged(Color old, Color curr)
    {
        if (isOwned)
        {
            PlayerManager.Instance.PlayerColor.color = curr;
            PlayerManager.Instance.PlayerColor.sprite = characterPics[pictureIndex];
        }

        UpdateInfo();
    }

    private void OnIndexChanged(int old, int curr)
    {
        print($"Index changed {netId} from {old} to {curr}");
        if (isOwned)
        {
            PlayerManager.Instance.PlayerColor.color = displayColor;
            PlayerManager.Instance.PlayerColor.sprite = characterPics[curr];
        }

        UpdateInfo();
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
        infoDisplay?.Display(this);
        if (DisplayEnt != null)
        {
            var boardFigureImage = DisplayEnt.GetComponent<Image>();
            boardFigureImage.sprite = characterPics[pictureIndex];
            boardFigureImage.color = displayColor;
        }

        if (TilePlacer.Instance != null && TilePlacer.Instance.Tiles != null)
        {
            TilePlacer.Instance.Tiles[Position].PlacePlayer(this);
        }
    }

    public bool CanBuyTile(Player player, TileData tileData)
    {
        return player.Position == tileData.position
               && player.ownedMoney >= tileData.baseTile.cost
               && PlayerManager.Instance.CurrentPlayer.netId == player.netId;
    }

    [Command]
    public void CmdBuyTile(int tilePosition)
    {
        var data = TilePlacer.Instance.GetTileAt(tilePosition);
        if (CanBuyTile(this, data))
        {
            ownedMoney -= data.baseTile.cost;
            TilePlacer.Instance.RpcBoughtTile(data.position, netId);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdCheckPosition()
    {
        var tileData = TilePlacer.Instance.GetTileAt(Position);
        print($"Checking {DisplayName} position {Position} with tile {tileData.baseTile.displayName}");
        if (!tileData.baseTile.government && tileData.isOwned && tileData.ownerId != netId)
        {
            var fee = tileData.fee;
            canPay = OwnedMoney >= fee;
            if (!canPay)
            {
                PropertySellHelper.Instance.RpcSellMyProperties(netId, fee);
            }

            StartCoroutine(WaitForPlayerHasMoney(tileData));
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

    private void OnPropertyAction(bool res)
    {
        if (!res)
        {
            PlayerManager.Instance.OnExitConfirmation();
        }
        else
        {
            canPay = true;
        }
    }

    private IEnumerator WaitForPlayerHasMoney(TileData tileData)
    {
        var fee = tileData.fee;
        yield return new WaitUntil(() => ownedMoney >= fee);
        var otherPlayer = PlayerManager.Instance.GetPlayerWithId(tileData.ownerId);
        otherPlayer.ServerSideOnlyUpdateMoney(fee);
        ownedMoney -= fee;
    }

    [ClientRpc]
    private void RpcCheckTile()
    {
        print("Checking for extra events");
        if (!isOwned || !PlayerManager.Instance.CurrentPlayer.isOwned) return;

        var tileData = TilePlacer.Instance.GetTileAt(Position);
        tileData.extraEvent.PlayerArrived(this, tileData);
    }

    [Command(requiresAuthority = false)]
    public void CmdSellTile(int tilePosition, int remainingAmount)
    {
        var tileData = TilePlacer.Instance.GetTileAt(tilePosition);
        if (tileData.isOwned && tileData.ownerId == netId)
        {
            ownedMoney += tileData.sellPrice;
            TilePlacer.Instance.RpcSoldTile(tilePosition);
            canPay = remainingAmount <= 0;
        }

        if (!canPay)
        {
            PropertySellHelper.Instance.RpcSellMyProperties(netId, remainingAmount);
        }
    }
}