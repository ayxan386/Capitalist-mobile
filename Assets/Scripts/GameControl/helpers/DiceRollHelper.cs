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
        [SerializeField] private Vector2Int rollRange = new Vector2Int(1, 7);

        public static DiceRollHelper Instance { get; private set; }

        public bool CanRoll { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        public void DiceRollButtonCall()
        {
            if (!PlayerManager.Instance.IsGameStarted || !CanRoll) return;
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
            // var diceRoll = Random.Range(rollRange.x, rollRange.y);
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
            beforeAnimationPlayer.eventMove = false;
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
                PlayerMovementHelper.Instance.CmdAnimatedMoveToTile(
                    TilePlacer.Instance.CalculatePosition(beforeAnimationPlayer.Position, diceRoll));
            }
        }
    }
}