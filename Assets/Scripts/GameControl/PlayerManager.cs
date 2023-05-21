using System;
using System.Collections.Generic;
using System.Linq;
using GameControl.helpers;
using Mirror;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameControl
{
    public class PlayerManager : NetworkBehaviour
    {
        [Header("waiting menu")] [SerializeField]
        private TextMeshProUGUI totalNumberDisplay;

        [SerializeField] private TMP_InputField nameInput;

        [SerializeField] private GameObject confirmationMenu;
        [SerializeField] private GameObject onGoingGameMenu;

        [Header("Indicators")] [SerializeField]
        private TextMeshProUGUI turnIndicatorText;

        private Dictionary<uint, Player> players = new();
        private int totalNumberOfReadyPlayers;

        [SyncVar(hook = nameof(OnTurnChanged))]
        private uint currentPlayersTurn;

        public static bool IsGameStarted { get; private set; }
        public static PlayerManager Instance { get; private set; }

        public Player CurrentPlayer => players[currentPlayersTurn];

        private Player prevPlayer;
        private Player firstPlayer;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Player.OnPlayerSpawned += OnPlayerSpawned;
        }

        private void OnTurnChanged(uint prevPlayerId, uint nextPlayerId)
        {
            CurrentPlayer.UpdateInfo();
            currentPlayersTurn = nextPlayerId;
            var player = players[nextPlayerId];
            print("is player owned: " + player.isOwned);
            DiceRollHelper.Instance.CanRoll = player.isOwned;
            turnIndicatorText.text = $"{player.DisplayName} turn";
        }

        public void OnPlayerReady()
        {
            var ownedPlayer = FindOwnedPlayer();
            CmdSetPlayerReady(nameInput.text, ownedPlayer);
        }

        [Command(requiresAuthority = false)]
        private void CmdSetPlayerReady(string displayName, uint ownedPlayer)
        {
            totalNumberOfReadyPlayers++;
            players[ownedPlayer].CmdUpdateDisplayName(displayName);
            if (totalNumberOfReadyPlayers == NetworkManager.singleton.numPlayers)
            {
                var firstPlayer = players.Values.ElementAt(Random.Range(0, totalNumberOfReadyPlayers));
                prevPlayer.NextPlayer = this.firstPlayer;
                currentPlayersTurn = firstPlayer.netId;
                RpcStartGame();
            }
            else
            {
                RpcPlayerReadyCountChange(totalNumberOfReadyPlayers);
            }
        }

        private uint FindOwnedPlayer()
        {
            foreach (var player in players.Values.Where(player => player.isOwned))
            {
                return player.netId;
            }

            throw new ArgumentException("No players found");
        }

        [ClientRpc]
        private void RpcStartGame()
        {
            print("All ready, start");
            confirmationMenu.SetActive(false);
            onGoingGameMenu.SetActive(true);
            IsGameStarted = true;
            foreach (var player in players.Values)
            {
                player.UpdateInfo();
            }
        }

        private void OnPlayerSpawned(Player obj)
        {
            if(obj.isOwned)
                nameInput.text = "Player " + obj.netId;
            players.Add(obj.netId, obj);
            firstPlayer ??= obj;
            prevPlayer ??= obj;
            prevPlayer.NextPlayer = obj;
            prevPlayer = obj;
        }

        [ClientRpc]
        public void RpcPlayerReadyCountChange(int newTotal)
        {
            totalNumberDisplay.text = newTotal + " players ready";
        }

        public void EndPlayerTurn()
        {
            if (CurrentPlayer.isOwned)
                CmdPlayerPlayedTurn(currentPlayersTurn);
        }

        [Command(requiresAuthority = false)]
        private void CmdPlayerPlayedTurn(uint playerId)
        {
            if (playerId != currentPlayersTurn) return;
            currentPlayersTurn = CurrentPlayer.NextPlayer.netId;

            if (players.Count == 1)
            {
                OnTurnChanged(currentPlayersTurn, currentPlayersTurn);
            }
        }
    }
}