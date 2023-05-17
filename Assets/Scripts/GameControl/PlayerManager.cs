using UnityEngine;

namespace GameControl
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private RectTransform player;
        [SerializeField] private int currentPos;

        [ContextMenu("Place player")]
        public void PlacePlayer()
        {
            TilePlacer.Instance.Tiles[currentPos].PlacePlayer(player);
        }

        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                PlacePlayer();
                currentPos++;
                currentPos %= TilePlacer.Instance.Tiles.Length;
            }
        }
    }
}