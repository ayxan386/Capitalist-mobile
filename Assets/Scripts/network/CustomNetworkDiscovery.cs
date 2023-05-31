using System;
using System.Net;
using GameControl;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.Events;

/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-discovery
    API Reference: https://mirror-networking.com/docs/api/Mirror.Discovery.NetworkDiscovery.html
*/

public struct DiscoveryRequest : NetworkMessage
{
    // Add public fields (not properties) for whatever information you want
    // sent by clients in their broadcast messages that servers will use.
}

public struct DiscoveryResponse : NetworkMessage
{
    public int numberOfPlayers;
    public string hostName;
    public bool canJoin;
    public long serverId;
    public Uri uri;

    public override string ToString()
    {
        return $"Server response :{hostName}, can join {canJoin}, uri {uri}";
    }
}

public class CustomNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
{
    [SerializeField] private UnityEvent<DiscoveryResponse> OnServerFoundCust;

    #region Unity Callbacks

#if UNITY_EDITOR
    public override void OnValidate()
    {
        base.OnValidate();
    }
#endif

    public override void Start()
    {
        base.Start();
    }

    #endregion

    #region Server

    protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
    {
        print("Client is looking for me");
        try
        {
            return new DiscoveryResponse
            {
                serverId = ServerId,
                uri = transport.ServerUri(),
                numberOfPlayers = NetworkManager.singleton.numPlayers,
                canJoin = PlayerManager.Instance != null && !PlayerManager.Instance.IsGameStarted,
                hostName = SystemInfo.deviceName
            };
        }
        catch (NotImplementedException)
        {
            Debug.LogError($"Transport {transport} does not support network discovery");
            throw;
        }
        catch (NullReferenceException)
        {
            Debug.LogError("Some useless error");
            throw;
        }
    }

    #endregion

    #region Client

    protected override DiscoveryRequest GetRequest() => new DiscoveryRequest();

    protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
    {
        print("Response received :" + response);
        if (!response.canJoin) return;


        UriBuilder realUri = new UriBuilder(response.uri)
        {
            Host = endpoint.Address.ToString()
        };

        response.uri = realUri.Uri;

        OnServerFoundCust?.Invoke(response);
    }

    #endregion
}