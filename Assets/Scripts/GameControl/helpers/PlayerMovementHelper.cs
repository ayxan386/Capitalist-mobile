using System;
using System.Collections;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameControl.helpers
{
    public class PlayerMovementHelper : NetworkBehaviour
    {
        [SerializeField] private AudioClip movementSoundClip;
        [SerializeField] private AudioSource audioSource;
        
        public static PlayerMovementHelper Instance { get; private set; }

        [SyncVar] private int selectedTile;
        private int completedAnimationCount = 0;

        public static event Action<int> OnSelectionComplete; 

        private void Awake()
        {
            Instance = this;
        }

        private IEnumerator MovePlayer(int dist, Player player)
        {
            for (int i = 0; i < dist; i++)
            {
                player.UpdatePositionServerSide(TilePlacer.Instance.CalculatePosition(player.Position, 1));
                audioSource.PlayOneShot(movementSoundClip);
                yield return new WaitForSeconds(0.3f);
            }

            player.CmdCheckPosition();
        }


        [Command(requiresAuthority = false)]
        public void CmdAnimatedMoveToTile(int selectedTile)
        {
            var currentPlayer = PlayerManager.Instance.CurrentPlayer;
            var tilesLength = TilePlacer.Instance.Tiles.Length;
            var dist = selectedTile - currentPlayer.Position;

            if (dist < -1)
            {
                dist += tilesLength;
            }

            StartCoroutine(MovePlayer(dist, currentPlayer));
        }

        [Command(requiresAuthority = false)]
        public void CmdDirectMoveToTile(int selectedTile)
        {
            var currentPlayer = PlayerManager.Instance.CurrentPlayer;
            currentPlayer.eventMove = true;
            var tilesLength = TilePlacer.Instance.Tiles.Length;
            var dist = selectedTile - currentPlayer.Position;

            if (dist < -1)
            {
                dist += tilesLength;
            }

            currentPlayer.UpdatePositionServerSide(TilePlacer.Instance.CalculatePosition(currentPlayer.Position, dist));
            audioSource.PlayOneShot(movementSoundClip);
        }

        [Command(requiresAuthority = false)]
        public void CmdSelectRandomTile()
        {
            var tilesLength = TilePlacer.Instance.Tiles.Length;
            selectedTile = Random.Range(0, tilesLength);
            completedAnimationCount = 0;
            TilePlacer.Instance.RpcAnimatedSelection(selectedTile);
        }

        [Command(requiresAuthority = false)]
        public void CmdAnimationComplete()
        {
            print("Animation complete: " + completedAnimationCount);
            completedAnimationCount++;

            if (completedAnimationCount == NetworkManager.singleton.numPlayers)
            {
                RpcInformOfCompletion();
            }
        }

        [ClientRpc]
        private void RpcInformOfCompletion()
        {
            OnSelectionComplete?.Invoke(selectedTile); 
        }
    }
}