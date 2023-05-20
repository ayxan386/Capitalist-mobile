using System.Collections;
using Mirror;
using TMPro;
using UnityEngine;

public class NetworkAddressDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI networkAddressText;

    private void Start()
    {
        networkAddressText.text = NetworkManager.singleton.networkAddress;
    }

    public void CopyToClipBoard()
    {
        GUIUtility.systemCopyBuffer = networkAddressText.text;
        StartCoroutine(PopUpText());
    }

    private IEnumerator PopUpText()
    {
        networkAddressText.text = "Copied";
        yield return new WaitForSeconds(0.4f);
        networkAddressText.text = NetworkManager.singleton.networkAddress;
    }
}