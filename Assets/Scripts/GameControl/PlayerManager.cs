using System.Collections.Generic;
using UnityEngine;

namespace GameControl
{
    public class PlayerManager : MonoBehaviour
    {
        private List<Player> players = new();

        private void Start()
        {
            Player.OnPlayerSpawned += OnPlayerSpawned;
            Player.OnPlayerPositionChanged += OnPlayerPositionChanged;
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

        [ContextMenu("Place player")]
        public void PlacePlayer(Player player)
        {
            TilePlacer.Instance.Tiles[player.Position].PlacePlayer(player);
        }
        //
        // private void Update()
        // {
        //     if (Input.GetKeyUp(KeyCode.Space))
        //     {
        //         PlacePlayer();
        //         currentPos++;
        //         currentPos %= TilePlacer.Instance.Tiles.Length;
        //     }
        // }
    }
}