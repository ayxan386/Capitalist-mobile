using System;
using System.Collections.Generic;
using GameControl;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class DepositHelper : NetworkBehaviour
{
    [SerializeField] private Vector2Int interestRateRange;
    public static DepositHelper Instance { get; private set; }

    private Dictionary<uint, int> playerInterestRates;
    private Dictionary<uint, DepositAmount> playerDeposits;

    public static event Action<int> OnInterestDecided;

    private void Awake()
    {
        Instance = this;
        playerInterestRates = new Dictionary<uint, int>();
        playerDeposits = new Dictionary<uint, DepositAmount>();
    }

    [Command(requiresAuthority = false)]
    public void CmdDecideInterestRateForPlayer(uint playerId)
    {
        var interestRate = Random.Range(interestRateRange.x, interestRateRange.y);
        playerInterestRates[playerId] = interestRate;
        RpcInterestDecided(interestRate);
    }

    [ClientRpc]
    private void RpcInterestDecided(int interestRate)
    {
        OnInterestDecided?.Invoke(interestRate);
    }

    [Command(requiresAuthority = false)]
    public void CmdOpenDeposit(uint playerId, int amount)
    {
        if (playerInterestRates.ContainsKey(playerId))
        {
            var player = PlayerManager.Instance.GetPlayerWithId(playerId);
            player.CmdUpdateOwnedMoney(player.OwnedMoney - amount);
            playerDeposits[playerId] = new DepositAmount(playerInterestRates[playerId], amount);
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdCheckForDeposit(uint playerId)
    {
        if (playerDeposits.ContainsKey(playerId))
        {
            var playerDeposit = playerDeposits[playerId];
            var player = PlayerManager.Instance.GetPlayerWithId(playerId);
            player.CmdUpdateOwnedMoney(player.OwnedMoney
                                       + (int)(playerDeposit.amount * (1 + 1f * playerDeposit.interestRate / 100)));
            playerDeposits.Remove(playerId);
        }
    }
}

public class DepositAmount
{
    public int interestRate;
    public int amount;

    public DepositAmount(int interestRate, int amount)
    {
        this.interestRate = interestRate;
        this.amount = amount;
    }
}