using Agoxandr.Utils;
using Mirror;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SteamworksTransport : Transport
{
    [Header("Relay Socket")]
    public bool useRelay = false;
    public ulong targetSteamId;
    [Header("Normal Socket")]
    public ushort port = 7777;
    [Header("Auth Server")]
    public bool checkIdentity = false;
    public ushort steamPort = 7778;
    public ushort queryPort = 7779;
    public string serverName = "Test server";
    internal bool isServer;
    private bool serverActive;
    public static SteamworksTransport instance;
    public ServerInterface Server { get; private set; }
    public ClientInterface Client { get; private set; }

    private void Awake()
    {
        instance = this;
    }

    public class NetworkPlayer
    {
        public Connection connection;
        public ConnectionInfo data;

        public NetworkPlayer(Connection connection, ConnectionInfo data, int index)
        {
            connection.UserData = index;
            this.connection = connection;
            this.data = data;
        }
    }

    public class ServerInterface : SocketInterface
    {
        public Dictionary<int, NetworkPlayer> clients = new Dictionary<int, NetworkPlayer>();
        private int index = 0;

        public override void OnConnected(Connection connection, ConnectionInfo data)
        {
            base.OnConnected(connection, data);
            clients.Add(++index, new NetworkPlayer(connection, data, index));
            instance.OnServerConnected.Invoke(index);
            if (Connected.Count == 1)
            {
                EventManager.OnUpdated += OnUpdated;
            }
        }

        public override void OnConnecting(Connection connection, ConnectionInfo data)
        {
            base.OnConnecting(connection, data);
        }

        public override void OnConnectionChanged(Connection connection, ConnectionInfo data)
        {
            switch (data.State)
            {
                case ConnectionState.Connecting:
                    OnConnecting(connection, data);
                    break;
                case ConnectionState.Connected:
                    OnConnected(connection, data);
                    break;
                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    OnDisconnected(connection, data);
                    break;
            }
            if (LogFilter.Debug)
            {
                Debug.Log($"Server {Socket} {connection} {data.State}");
            }
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data)
        {
            if (Connected.Count == 1)
            {
                EventManager.OnUpdated -= OnUpdated;
            }
            instance.OnServerDisconnected.Invoke((int)connection.UserData);
            base.OnDisconnected(connection, data);
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            var buffer = new byte[size];
            Marshal.Copy(data, buffer, 0, size);
            instance.OnServerDataReceived.Invoke((int)connection.UserData, new ArraySegment<byte>(buffer), channel);
        }

        private void OnUpdated()
        {
            Receive();
        }
    }

    public class ClientInterface : ConnectionInterface
    {
        public override void OnConnected(ConnectionInfo data)
        {
            base.OnConnected(data);
            instance.OnClientConnected.Invoke();
            EventManager.OnUpdated += OnUpdated;
        }

        public override void OnConnecting(ConnectionInfo data)
        {
            base.OnConnecting(data);
        }

        public override void OnConnectionChanged(ConnectionInfo data)
        {
            switch (data.State)
            {
                case ConnectionState.Connecting:
                    OnConnecting(data);
                    break;
                case ConnectionState.Connected:
                    OnConnected(data);
                    break;
                case ConnectionState.ClosedByPeer:
                case ConnectionState.ProblemDetectedLocally:
                    OnDisconnected(data);
                    break;
            }
            if (LogFilter.Debug)
            {
                Debug.Log($"Client {Connection} {data.State}");
            }
        }

        public override void OnDisconnected(ConnectionInfo data)
        {
            EventManager.OnUpdated -= OnUpdated;
            instance.OnClientDisconnected.Invoke();
            base.OnDisconnected(data);
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            var buffer = new byte[size];
            Marshal.Copy(data, buffer, 0, size);
            instance.OnClientDataReceived.Invoke(new ArraySegment<byte>(buffer), channel);
        }

        private void OnUpdated()
        {
            Receive();
        }
    }

    public override bool Available()
    {
#if UNITY_64
        return true;
#else
        return false;
#endif
    }

    public override void ClientConnect(string address)
    {
        if (useRelay) Client = SteamNetworkingSockets.ConnectRelay<ClientInterface>(targetSteamId);
        else Client = SteamNetworkingSockets.ConnectNormal<ClientInterface>(NetAddress.From(address, port));
    }

    public override bool ClientConnected()
    {
        return Client.Connected;
    }

    public override void ClientDisconnect()
    {
        if (Client != null)
        {
            Client.Connection.Flush();
            Client.Connection.Close();
        }
    }

    public override bool ClientSend(int channelId, ArraySegment<byte> segment)
    {
        Client.Connection.SendMessage(segment.Array, segment.Offset, segment.Count, (SendType)channelId);
        return true;
    }

    public override int GetMaxPacketSize(int channelId = 8)
    {
        switch (channelId)
        {
            case (int)SendType.Unreliable:
            case (int)SendType.NoDelay:
                return 1200;
            case (int)SendType.Reliable:
            case (int)SendType.NoNagle:
                return 1048576;
            default:
                throw new NotSupportedException();
        }
    }

    public override bool ServerActive()
    {
        return serverActive;
    }

    public override bool ServerDisconnect(int connectionId)
    {
        return Server.clients[connectionId].connection.Close();
    }

    public override string ServerGetClientAddress(int connectionId)
    {
        return Server.clients[connectionId].connection.ConnectionName;
    }

    public override bool ServerSend(List<int> connectionIds, int channelId, ArraySegment<byte> segment)
    {
        for (int i = 0; i < connectionIds.Count; i++)
        {
            Server.clients[connectionIds[i]].connection.SendMessage(segment.Array, segment.Offset, segment.Count, (SendType)channelId);
        }
        return true;
    }

    public override void ServerStart()
    {
        isServer = true;
        SteamServerManager.instance = new SteamServerManager();
        SteamServerManager.instance.Initialize(port, steamPort, queryPort, serverName, NetworkManager.singleton.maxConnections);
        StartCoroutine(StartServerRoutine());
    }

    private IEnumerator StartServerRoutine()
    {
        yield return new WaitUntil(() => SteamServer.IsValid);
        if (useRelay) Server = SteamNetworkingSockets.CreateRelaySocket<ServerInterface>();
        else Server = SteamNetworkingSockets.CreateNormalSocket<ServerInterface>(NetAddress.AnyIp(port));
        serverActive = true;
    }

    public override void ServerStop()
    {
        if (Server != null)
        {
            for (int i = 0; i < Server.Connected.Count; i++)
            {
                Server.Connected[i].Flush();
                Server.Connected[i].Close();
            }
            Server.Close();
            SteamServer.Shutdown();
            Server = null;
        }
    }

    public override Uri ServerUri()
    {
        throw new NotImplementedException();
    }

    public override void Shutdown()
    {
        if (isServer)
        {
            if (serverActive)
            {
                ServerStop();
            }
        }
        else
        {
            if (Client != null)
            {
                if (Client.Connected)
                {
                    ClientDisconnect();
                }
            }
        }
    }
}
