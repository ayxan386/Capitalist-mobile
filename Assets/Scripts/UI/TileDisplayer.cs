using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileDisplayer : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI displayName;

    public void Display(TileData data)
    {
        icon.sprite = data.icon;
        displayName.text = data.displayName;
    }
}