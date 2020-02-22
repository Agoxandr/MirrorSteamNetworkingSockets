using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Text;
using UnityEngine;

public class AlwaysInclude : MonoBehaviour
{
    public GameObject steamClientManager;

    private void Start()
    {
        LogFilter.Debug = true;
#if UNITY_EDITOR
        if (PlayerPrefs.GetInt("Run") == 0)
        {
            StartCoroutine(StartClientRoutine());
        }
        else if (PlayerPrefs.GetInt("Run") == 1)
        {
            StartServer();
        }
#else
#if UNITY_SERVER
        StartServer();
#else
        StartCoroutine(StartClientRoutine());
#endif
#endif
    }

    private IEnumerator StartClientRoutine()
    {
        Instantiate(steamClientManager);
        yield return new WaitUntil(() => SteamClient.IsValid);
        NetworkManager.singleton.StartClient();
    }

    private void StartServer()
    {
        try
        {
            NetworkManager.singleton.StartServer();
        }
        catch (Exception exception)
        {
            Debug.Log(new StringBuilder(exception.Message).Append(" Shut down the server and fix or remove the config.json").ToString());
            throw;
        }
    }
}
