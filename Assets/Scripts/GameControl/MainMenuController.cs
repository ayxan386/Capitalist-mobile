using GameControl;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TMP_InputField numberOfPlayers;

    public void StartGame()
    {
        PlayerPrefs.SetInt(GlobalConstants.NumberOfPlayersKey, int.Parse(numberOfPlayers.text));

        SceneManager.LoadScene("SampleScene");
    }
}