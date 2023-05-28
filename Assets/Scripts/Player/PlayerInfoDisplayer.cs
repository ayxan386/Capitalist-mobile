using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoDisplayer : MonoBehaviour
{
    [SerializeField] private Image profilePicture;
    [SerializeField] private TextMeshProUGUI displayName;
    [SerializeField] private TextMeshProUGUI ownedMoney;

    public void Display(Player player)
    {
        // profilePicture.sprite = player.ProfilePicture;
        profilePicture.color = player.displayColor;
        displayName.text = player.DisplayName;
        ownedMoney.text = $"${player.OwnedMoney}";
    }

}