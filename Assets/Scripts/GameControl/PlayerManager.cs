using System;
using System.Collections.Generic;
using System.Linq;
using GameControl.helpers;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        [SerializeField] private Transform playersHolder;
        [SerializeField] private Transform otherPlayersDisplayers;
        [SerializeField] private Transform selfPlayersDisplayer;
        [SerializeField] private Image playerColor;

        [Header("Indicators")] [SerializeField]
        private TextMeshProUGUI turnIndicatorText;

        [SerializeField] private AudioSource soundSource;
        [SerializeField] private GameObject exitConfirmation;

        private Dictionary<uint, Player> players = new();
        private int totalNumberOfReadyPlayers;

        [SyncVar(hook = nameof(OnTurnChanged))]
        private uint currentPlayersTurn;

        public static bool IsGameStarted { get; private set; }
        public static PlayerManager Instance { get; private set; }

        public AudioSource SfxSource => soundSource;

        public Player CurrentPlayer => players[currentPlayersTurn];
        public Player OwnedPlayer => players[FindOwnedPlayer()];

        public Transform PlayersHolder => playersHolder;
        public Transform OtherPlayersDisplayers => otherPlayersDisplayers;
        public Transform SelfPlayersDisplayer => selfPlayersDisplayer;

        public Image PlayerColor => playerColor;

        private Player prevPlayer;
        private Player firstPlayer;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            Player.OnPlayerSpawned += OnPlayerSpawned;
            Player.OnPlayerDespawned += OnPlayerDespawned;
        }


        private void OnTurnChanged(uint prevPlayerId, uint nextPlayerId)
        {
            CurrentPlayer.UpdateInfo();
            currentPlayersTurn = nextPlayerId;
            var player = players[nextPlayerId];
            DiceRollHelper.Instance.CanRoll = player.isOwned;
            turnIndicatorText.text = $"{player.DisplayName} turn";

            if (player.isOwned)
            {
                Handheld.Vibrate();
            }
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

        private void OnPlayerSpawned(Player newPlayer)
        {
            if (newPlayer.isOwned)
            {
                nameInput.text = "Player " + newPlayer.netId;
                if (IsGameStarted && NetworkManager.singleton.isNetworkActive)
                {
                    NetworkManager.singleton.StopClient();
                }
            }

            players.Add(newPlayer.netId, newPlayer);
            firstPlayer ??= newPlayer;
            prevPlayer ??= newPlayer;
            prevPlayer.NextPlayer = newPlayer;
            newPlayer.PrevPlayer = prevPlayer;
            prevPlayer = newPlayer;
        }

        private void OnPlayerDespawned(Player disconnectedPlayer)
        {
            if (currentPlayersTurn == disconnectedPlayer.netId)
            {
                EndPlayerTurn();
            }

            players.Remove(disconnectedPlayer.netId);
            disconnectedPlayer.PrevPlayer.NextPlayer = disconnectedPlayer.NextPlayer;
            // var curr = firstPlayer;
            // var prev = curr;
            // while (curr.netId != disconnectedPlayer.netId && curr.NextPlayer.netId != firstPlayer.netId)
            // {
            //     prev = curr;
            //     curr = curr.NextPlayer;
            // }
            //
            // prev.NextPlayer = curr.NextPlayer;
        }

        [ClientRpc]
        public void RpcPlayerReadyCountChange(int newTotal)
        {
            totalNumberDisplay.text = newTotal + " players ready";
            foreach (var player in players.Values)
            {
                player.UpdateInfo();
            }
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
            RpcPlayerResetAndUpdate();

            if (players.Count == 1)
            {
                OnTurnChanged(currentPlayersTurn, currentPlayersTurn);
            }
        }


        [ClientRpc]
        public void RpcPlayerResetAndUpdate()
        {
            foreach (var player in players.Values)
            {
                player.LastChange = 0;
                player.UpdateInfo();
            }
        }

        public Player GetPlayerWithId(uint playerId)
        {
            return players[playerId];
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                exitConfirmation.SetActive(true);
            }
        }

        public void OnExitConfirmation()
        {
            if (isServer)
                NetworkManager.singleton.StopHost();
            else
                NetworkManager.singleton.StopClient();
        }
    }
}