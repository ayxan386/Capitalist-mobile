using System;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameControl
{
    public class PlayerManager : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI totalNumberDisplay;
        [SerializeField] private GameObject confirmationMenu;
        private List<Player> players = new();
        private int totalNumberOfReadyPlayers;
        private uint currentPlayersTurn;

        public static bool IsGameStarted { get; private set; }
        public static event Action<uint> OnTurnChange;

        private void Start()
        {
            Player.OnPlayerSpawned += OnPlayerSpawned;
            Player.OnPlayerPositionChanged += OnPlayerPositionChanged;
            Player.OnPlayerPlayed += OnPlayerPlayed;
        }

        private void OnPlayerPlayed(Player obj)
        {
            PlayerPlayedTurn(obj.netId);
        }

        public void OnPlayerReady()
        {
            CmdSetPlayerReady();
        }

        [Command(requiresAuthority = false)]
        private void CmdSetPlayerReady()
        {
            totalNumberOfReadyPlayers++;
            if (totalNumberOfReadyPlayers == NetworkManager.singleton.numPlayers)
            {
                var firstPlayer = players[Random.Range(0, totalNumberOfReadyPlayers)];
                RpcStartGame(firstPlayer.netId);
            }
            else
            {
                RpcPlayerReadyCountChange(totalNumberOfReadyPlayers);
            }
        }

        [ClientRpc]
        private void RpcStartGame(uint firstPlayer)
        {
            print("All ready, start");
            currentPlayersTurn = firstPlayer;
            OnTurnChange?.Invoke(currentPlayersTurn);
            confirmationMenu.SetActive(false);
            IsGameStarted = true;
        }

        private void OnPlayerPositionChanged(Player obj)
        {
            PlacePlayer(obj);
        }

        private void OnPlayerSpawned(Player obj)
        {
            players.Add(obj);
            PlacePlayer(obj);
        }

        public void PlacePlayer(Player player)
        {
            TilePlacer.Instance.Tiles[player.Position].PlacePlayer(player);
        }

        [ClientRpc]
        public void RpcPlayerReadyCountChange(int newTotal)
        {
            totalNumberDisplay.text = newTotal + " players ready";
        }
        
        [ClientRpc]
        public void RpcPlayerTurnChanged(uint newPlayer)
        {
            OnTurnChange?.Invoke(newPlayer);
        }

        private void PlayerPlayedTurn(uint playerId)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].netId == playerId)
                {
                    RpcPlayerTurnChanged(players[(i + 1) % players.Count].netId); 
                }
            }
        } 
    }
}