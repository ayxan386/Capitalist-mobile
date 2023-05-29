using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfoDisplayer : MonoBehaviour
{
    [SerializeField] private Image profilePicture;
    [SerializeField] private TextMeshProUGUI displayName;
    [SerializeField] private TextMeshProUGUI ownedMoney;
    [SerializeField] private TextMeshProUGUI moneyChangeText;
    [SerializeField] private Color positiveChange;
    [SerializeField] private Color negativeChange;

    public void Display(Player player)
    {
        // profilePicture.sprite = player.ProfilePicture;
        profilePicture.color = player.displayColor;
        displayName.text = player.DisplayName;
        ownedMoney.text = $"${player.OwnedMoney}";
        moneyChangeText.text = $"${player.LastChange}";
        moneyChangeText.color = player.LastChange > 0 ? positiveChange : negativeChange;
        moneyChangeText.alpha = player.LastChange != 0 ? 1 : 0;
    }
}