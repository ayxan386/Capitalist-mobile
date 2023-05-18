using System;
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
    public static event Action<Player> OnPlayerPositionChanged;

    public override void OnStartLocalPlayer()
    {
        position = 0;
        displayName = Guid.NewGuid().ToString();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        var holder = GameObject.Find("Players");
        DisplayEnt = Instantiate(playerDisplay, holder.transform);
        OnPlayerSpawned?.Invoke(this);
    }


    private void Update()
    {
        if (!isLocalPlayer) return;

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
    }


    public override void OnSerialize(NetworkWriter writer, bool initialState)
    {
        base.OnSerialize(writer, initialState);
    }

    public override void OnDeserialize(NetworkReader reader, bool initialState)
    {
        base.OnDeserialize(reader, initialState);
    }
}