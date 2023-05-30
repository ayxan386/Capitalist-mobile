using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown servers;

    private List<DiscoveryResponse> serversList;

    private IEnumerator Start()
    {
        while (true)
        {
            servers.ClearOptions();
            serversList = new List<DiscoveryResponse>();
            yield return new WaitForSeconds(12);
        }
    }

    public void StartGame()
    {
        var uri = serversList[servers.value].uri;
        NetworkLayer.Instance.JoinGame(uri);
    }

    public void HostGame()
    {
        NetworkLayer.Instance.StartHost();
    }

    public void OnHostFound(DiscoveryResponse response)
    {
        var foundIndex = serversList.FindIndex(server => server.serverId == response.serverId);
        if (foundIndex >= 0)
        {
            serversList.RemoveAt(foundIndex);
            servers.options.RemoveAt(foundIndex);
        }

        serversList.Add(response);
        servers.options.Add(new TMP_Dropdown.OptionData(response.hostName));

        if (serversList.Count == 1)
            servers.RefreshShownValue();
    }
}