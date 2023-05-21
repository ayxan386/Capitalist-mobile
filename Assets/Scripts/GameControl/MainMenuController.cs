using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TMP_InputField numberOfPlayers;

    public void StartGame()
    {
        // PlayerPrefs.SetInt(GlobalConstants.NumberOfPlayersKey, int.Parse(numberOfPlayers.text));
        NetworkLayer.Instance.JoinGame(numberOfPlayers.text);
        // SceneManager.LoadScene("SampleScene");
    }

    public void HostGame()
    {
        NetworkLayer.Instance.StartHost();
        // SceneManager.LoadScene("SampleScene");
    }
}