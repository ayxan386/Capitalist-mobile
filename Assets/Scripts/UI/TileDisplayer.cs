using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileDisplayer : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI displayName;


    public void Display(TileVariant data)
    {
        icon.sprite = data.icon;
        icon.color = data.spriteColor;
        displayName.text = data.displayName;
    }
}