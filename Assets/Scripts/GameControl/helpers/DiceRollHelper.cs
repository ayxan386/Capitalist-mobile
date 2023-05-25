﻿using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameControl.helpers
{
    public class DiceRollHelper : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI diceRollText;
        [SerializeField] private Vector3 rotationSpeed;
        [SerializeField] private GameObject diceRollBackground;
        [SerializeField] private Button endTurnButton;

        public static DiceRollHelper Instance { get; private set; }

        public bool CanRoll { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        public void DiceRollButtonCall()
        {
            if (!PlayerManager.IsGameStarted || !CanRoll) return;
            var player = PlayerManager.Instance.CurrentPlayer;
            if (player.isOwned)
            {
                CanRoll = false;
                CmdRollDice();
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdRollDice()
        {
            // var diceRoll = Random.Range(1, 7);
            var diceRoll = 5;
            RpcDiceRolled(diceRoll);
        }

        [ClientRpc]
        private void RpcDiceRolled(int diceRoll)
        {
            print("Dice rolled rpc received: " + diceRoll);
            StartCoroutine(AnimateDiceThenMove(diceRoll));
        }

        private IEnumerator AnimateDiceThenMove(int diceRoll)
        {
            diceRollText.text = diceRoll.ToString();
            var beforeAnimationPlayer = PlayerManager.Instance.CurrentPlayer;
            int duration = (int)(1 / Time.deltaTime) * 3 / 4;
            print("Animation duration is " + duration);
            for (int i = 0; i < duration; i++)
            {
                diceRollBackground.transform.Rotate(rotationSpeed * (60 * Time.deltaTime));
                yield return null;
            }

            diceRollBackground.transform.rotation = Quaternion.identity;
            if (beforeAnimationPlayer.isOwned)
            {
                StartCoroutine(MovePlayer(diceRoll, beforeAnimationPlayer));
            }
        }

        public IEnumerator MovePlayer(int dist, Player player)
        {
            for (int i = 0; i < dist; i++)
            {
                player.CmdUpdatePosition(TilePlacer.Instance.CalculatePosition(player.Position, 1));
                yield return new WaitForSeconds(0.3f);
            }

            player.CmdCheckPosition();
            endTurnButton.interactable = true;
        }


        [Command(requiresAuthority = false)]
        public void CmdMoveToTile(int selectedTile)
        {
            var currentPlayer = PlayerManager.Instance.CurrentPlayer;
            currentPlayer.eventMove = true;
            var tilesLength = TilePlacer.Instance.Tiles.Length;
            var dist = selectedTile - currentPlayer.Position;

            if (dist < -1)
            {
                dist += tilesLength;
            }

            StartCoroutine(MovePlayer(dist, currentPlayer));
        }
    }
}