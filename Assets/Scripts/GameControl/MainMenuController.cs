using Mirror.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameOfHost;

    public void StartGame()
    {
        // PlayerPrefs.SetInt(GlobalConstants.NumberOfPlayersKey, int.Parse(numberOfPlayers.text));
        NetworkLayer.Instance.JoinGame(nameOfHost.text);
        // SceneManager.LoadScene("SampleScene");
    }

    public void HostGame()
    {
        NetworkLayer.Instance.StartHost();
        // SceneManager.LoadScene("SampleScene");
    }

    public void OnHostFound(ServerResponse response)
    {
        nameOfHost.text = response.uri.ToString();
    }
}