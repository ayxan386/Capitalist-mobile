using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DepositTile : BaseTile
{
    private GameObject depositMenu;
    private Button confirmationButton;
    private Button cancelButton;
    private TMP_InputField amountField;
    private TextMeshProUGUI interestRateText;
    private Player activatedPlayer;
    private int amount;

    private void Start()
    {
        depositMenu = GameObject.Find("DepositMenu");
        confirmationButton = depositMenu.transform.Find("Confirmation").GetComponent<Button>();
        confirmationButton.onClick.AddListener(ConfirmDeposit);
        cancelButton = depositMenu.transform.Find("Cancel").GetComponent<Button>();
        cancelButton.onClick.AddListener(Cancel);
        amountField = depositMenu.transform.Find("AmountField").GetComponent<TMP_InputField>();
        amountField.onValueChanged.AddListener(OnAmountChanged);
        interestRateText = depositMenu.transform.Find("InterestRateText").GetComponent<TextMeshProUGUI>();
        DepositHelper.OnInterestDecided += DepositHelperOnInterestDecided;
    }

    private void OnDisable()
    {
        DepositHelper.OnInterestDecided -= DepositHelperOnInterestDecided;
    }

    private void DepositHelperOnInterestDecided(int interestRate)
    {
        interestRateText.text = $"Obtain {interestRate}% interest after next cycle";
    }

    private void OnAmountChanged(string amountStr)
    {
        amount = int.Parse(amountStr);
        if (amount > activatedPlayer.OwnedMoney)
        {
            amountField.text = activatedPlayer.OwnedMoney.ToString();
        }

        confirmationButton.interactable = true;
    }

    private void Cancel()
    {
        depositMenu.transform.localScale = Vector3.zero;
    }

    private void ConfirmDeposit()
    {
        depositMenu.transform.localScale = Vector3.zero;
        DepositHelper.Instance.CmdOpenDeposit(activatedPlayer.netId, amount);
    }

    public override void PlayerArrived(Player player, TileData selfData)
    {
        activatedPlayer = player;
        amountField.text = "1";
        depositMenu.transform.localScale = Vector3.one;
        DepositHelper.Instance.CmdDecideInterestRateForPlayer(player.netId);
    }
}