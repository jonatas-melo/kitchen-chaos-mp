using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public class KitchenGameLobby : MonoBehaviour
{
    public static KitchenGameLobby Instance { get; private set; }

    private Lobby joinedLobby;
    private float heartbeatTimer;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State == ServicesInitializationState.Initialized) return;

        var options = new InitializationOptions();
        options.SetProfile(Random.Range(0, 1000).ToString());

        await UnityServices.InitializeAsync(options);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleHeartBeat();
    }

    private void HandleHeartBeat()
    {
        if (!IsLobbyHost()) return;

        heartbeatTimer -= Time.deltaTime;
        if (!(heartbeatTimer <= 0f)) return;
        
        const float heartbeatTimerMax = 15f;
        heartbeatTimer = heartbeatTimerMax;
        LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
    }

    private bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName,
                KitchenObjectMultiplayer.MaxPlayerAmount,
                new CreateLobbyOptions()
                {
                    IsPrivate = isPrivate,
                });

            KitchenObjectMultiplayer.Instance.StartHost();
            Loader.LoadNetwork(Loader.Scene.CharacterSelectScene);
        }
        catch (LobbyServiceException err)
        {
            Debug.Log(err);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            KitchenObjectMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException err)
        {
            Debug.Log(err);
        }
    }

    public async void JoinWithCode(string lobbyCode)
    {
        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            KitchenObjectMultiplayer.Instance.StartClient();
        }
        catch (LobbyServiceException err)
        {
            Debug.Log(err);
        }
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }
}