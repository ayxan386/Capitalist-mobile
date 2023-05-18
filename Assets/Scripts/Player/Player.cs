using System;
using System.Collections;
using GameControl;
using Mirror;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private RectTransform playerDisplay;

    [SyncVar(hook = nameof(OnPositionChanged))]
    private int position;

    [SyncVar] private string displayName;

    public int Position => position;
    public string DisplayName => displayName;
    public RectTransform DisplayEnt { get; private set; }

    public static Action<Player> OnPlayerSpawned;
    private static TileVariant[] boardData;
    private bool myTurn;
    public static event Action<Player> OnPlayerPositionChanged;
    public static event Action<Player> OnPlayerReady;
    public static event Action<Player> OnPlayerPlayed;

    public override void OnStartLocalPlayer()
    {
        position = 0;
        displayName = Guid.NewGuid().ToString();
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

    private void OnTurnChange(uint playerId)
    {
        myTurn = playerId == netId;
        
        print("Player " + playerId + " turn");
    }

    private IEnumerator WaitForBoardThenPlace()
    {
        yield return new WaitUntil(() => TilePlacer.IsInitializeComplete);

        var holder = GameObject.Find("Players");
        DisplayEnt = Instantiate(playerDisplay, holder.transform);
        OnPlayerSpawned?.Invoke(this);
        
        PlayerManager.OnTurnChange += OnTurnChange;
    }

    [ClientRpc]
    private void RpcDisplayBoard(TileVariant[] boardData)
    {
        TilePlacer.Instance.PlaceTiles(boardData);
    }

    private void Update()
    {
        if (!isLocalPlayer || !PlayerManager.IsGameStarted) return;
        
        if(!myTurn) return;

        if (Input.GetMouseButtonDown(0))
        {
            CmdUpdatePosition(TilePlacer.Instance.CalculatePosition(Position, 1));
        }
    }

    private void OnPositionChanged(int old, int current)
    {
        OnPlayerPositionChanged?.Invoke(this);
    }

    [Command]
    private void CmdUpdatePosition(int newPos)
    {
        position = newPos;
        OnPlayerPlayed?.Invoke(this);
    }
}