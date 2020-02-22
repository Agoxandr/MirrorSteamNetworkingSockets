using Steamworks;
using System;
using System.Text;
using UnityEngine;

public class SteamClientManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        try
        {
            SteamClient.Init(923440);
        }
        catch (Exception exception)
        {
            Debug.LogWarning(exception.Message);
        }
        Debug.Log(new StringBuilder("Steam initialized: ").Append(SteamClient.Name).Append(" / ").Append(SteamClient.SteamId).ToString());
    }

    private void OnDestroy()
    {
        SteamClient.Shutdown();
    }
}
