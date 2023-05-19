using System.Collections;
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
            var diceRoll = Random.Range(1, 7);
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
            for (int i = 0; i < 45; i++)
            {
                diceRollBackground.transform.Rotate(rotationSpeed);
                yield return new WaitForEndOfFrame();
            }

            diceRollBackground.transform.rotation = Quaternion.identity;
            if (beforeAnimationPlayer.isOwned)
            {
                StartCoroutine(MovePlayer(diceRoll, beforeAnimationPlayer));
            }
        }

        private IEnumerator MovePlayer(int diceRoll, Player player)
        {
            for (int i = 0; i < diceRoll; i++)
            {
                player.CmdUpdatePosition(TilePlacer.Instance.CalculatePosition(player.Position, 1));
                yield return new WaitForSeconds(0.3f);
            }
            endTurnButton.interactable = true;
        }
    }
}