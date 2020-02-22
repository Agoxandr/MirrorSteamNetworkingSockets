using Steamworks;
using UnityEngine;

public class SteamServerManager
{
    public static SteamServerManager instance;

    public void Initialize(ushort gamePort, ushort steamPort, ushort queryPort, string serverName, int maxPlayers)
    {
        try
        {
            System.IO.File.WriteAllText("steam_appid.txt", "923440");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Couldn't write steam_appid.txt: " + e.Message);
        }

        var init = new SteamServerInit("Revision", "Revision")
        {
            GamePort = gamePort,
            SteamPort = steamPort,
            QueryPort = queryPort,
            VersionString = "1.0",
            Secure = true
        };

        try
        {
            SteamServer.Init(923440, init);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
        SteamServer.ServerName = serverName;
        SteamServer.MaxPlayers = maxPlayers;
        SteamServer.Passworded = false;
        SteamServer.DedicatedServer = true;

        SteamServer.OnValidateAuthTicketResponse += OnValidateAuthTicketResponse;
        SteamServer.LogOnAnonymous();
    }

    public void ValidatePlayer(ulong steamId, byte[] data)
    {
        Debug.Log(SteamServer.BeginAuthSession(data, steamId));
    }

    public void OnValidateAuthTicketResponse(SteamId steamID, SteamId ownerId, AuthResponse status)
    {
        //foreach (var client in NetworkManager.instance.Server.clients)
        //{
        //    if (client.Value.steamId == steamID)
        //    {
        //        if (status == AuthResponse.OK)
        //        {
        //            if (LogFilter.Debug) Debug.Log(status.ToString());
        //            client.Value.vacOk = true;
        //            if (!client.Value.playerInitialized)
        //            {
        //                NetworkManager.instance.Server.ServerSendAllowSpawn(client.Value.connection);
        //            }
        //            else
        //            {
        //                if (client.Value.core.health.isAlive)
        //                {
        //                    NetworkManager.instance.Server.ServerSendReconnectPlayer(client.Value.connection, client.Value.core.transform.localPosition, client.Value.core.move.horizontal, client.Value.core.rb.velocity, (byte)client.Value.teamIndex);
        //                }
        //                else
        //                {
        //                    // Player dead
        //                    NetworkManager.instance.Server.ServerSendAllowSpawn(client.Value.connection);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (NetworkManager.instance.logLevel >= LogLevel.Event) Debug.Log(status.ToString());
        //            if (status == AuthResponse.VACCheckTimedOut)
        //            {

        //            }
        //            else
        //            {
        //                client.Value.connection.Close(false, status, status.ToString());
        //            }
        //        }
        //    }
        //}
    }
}
