using System;
using System.Collections;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private RectTransform playerDisplay;
    [SerializeField] private PlayerInfoDisplayer infoDisplayPrefab;
    private PlayerInfoDisplayer infoDisplay;

    [SyncVar(hook = nameof(OnPositionChanged))]
    private int position;

    [SyncVar] private string displayName;

    public int Position => position;
    public string DisplayName => displayName;
    public RectTransform DisplayEnt { get; private set; }
    public Player NextPlayer { get; set; }

    public static Action<Player> OnPlayerSpawned;
    private static TileVariant[] boardData;
    public static event Action<Player> OnPlayerPositionChanged;

    public override void OnStartLocalPlayer()
    {
        position = 0;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isServer)
        {
            boardData ??= TilePlacer.Instance.GenerateBoardData();
            RpcDisplayBoard(boardData);
        }

        StartCoroutine(WaitForBoardThenPlace());
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

        OnPlayerSpawned?.Invoke(this);
    }

    [ClientRpc]
    private void RpcDisplayBoard(TileVariant[] boardData)
    {
        TilePlacer.Instance.PlaceTiles(boardData);
    }

    private void OnPositionChanged(int old, int current)
    {
        OnPlayerPositionChanged?.Invoke(this);
    }

    [Command]
    public void CmdUpdatePosition(int newPos)
    {
        position = newPos;
    }

    public void CmdUpdateDisplayName(string newName)
    {
        displayName = newName;
    }

    public void UpdateInfo()
    {
        infoDisplay.Display(this);
    }
}