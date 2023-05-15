using UnityEngine;

namespace GameControl
{
    public class PlayerManager : MonoBehaviour
    {
        [SerializeField] private RectTransform player;
        [SerializeField] private int currentPos;

        [ContextMenu("Place player")]
        public void Move()
        {
            TilePlacer.Instance.Tiles[currentPos].PlacePlayer(player);
        }
    }
}