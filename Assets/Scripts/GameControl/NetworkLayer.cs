using System;
using System.Net;
using System.Net.Sockets;
using Mirror;
using UnityEngine;

public class NetworkLayer : MonoBehaviour
{
    public static NetworkLayer Instance { get; private set; }

    [SerializeField] private NetworkManager networkManager;


    private void Awake()
    {
        Instance = this;
    }

    public void StartHost()
    {
        networkManager.networkAddress = GetLocalIPAddress();
        print("Network address: " + networkManager.networkAddress);
        networkManager.StartHost();
    }

    public void JoinGame(string ip)
    {
        networkManager.networkAddress = ip;
        print("Joining game: " + networkManager.networkAddress);
        networkManager.StartClient();
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new Exception("No network adapters with an IPv4 address in the system!");
    }
}