using System;
using System.Net;
using System.Net.Sockets;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class NetworkLayer : MonoBehaviour
{
    public static NetworkLayer Instance { get; private set; }

    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private CustomNetworkDiscovery networkDiscovery;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
       networkDiscovery.StartDiscovery(); 
    }

    public void StartHost()
    {
        networkManager.networkAddress = GetLocalIPAddress();
        print("Network address: " + networkManager.networkAddress);
        networkManager.StartHost();
        networkDiscovery.AdvertiseServer();
    }

    public void JoinGame(Uri ip)
    {
        networkManager.StartClient(ip);
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