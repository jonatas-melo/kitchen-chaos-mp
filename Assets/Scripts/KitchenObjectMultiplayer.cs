using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class KitchenObjectMultiplayer : NetworkBehaviour
{
    public const int MaxPlayerAmount = 4;
    private const string PlayerPrefsPlayerNameMultiplayer = "PlayerPrefsPlayerNameMultiplayer";

    public static KitchenObjectMultiplayer Instance { get; private set; }

    public event EventHandler OnTryingToJoinGame;
    public event EventHandler OnFailedToJoinGame;
    public event EventHandler OnPlayerDataNetworkListChanged;

    [SerializeField] private KitchenObjectListSO kitchenObjectListSo;
    [SerializeField] private List<Color> playerColorList;

    private NetworkList<PlayerData> playerDataNetworkList;
    private string playerName;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerName = PlayerPrefs.GetString(PlayerPrefsPlayerNameMultiplayer, "PlayerName" + Random.Range(100, 1000));
        playerDataNetworkList = new NetworkList<PlayerData>();
        playerDataNetworkList.OnListChanged += PlayerDataNetworkListOnListChanged;
    }

    public string GetPlayerName()
    {
        return playerName;
    }

    public void SetPlayerName(string newPlayerName)
    {
        playerName = newPlayerName;
        PlayerPrefs.SetString(PlayerPrefsPlayerNameMultiplayer, playerName);
    }

    private void PlayerDataNetworkListOnListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManagerConnectionApprovalCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManagerOnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManagerServerOnClientDisconnectCallback;
        NetworkManager.Singleton.StartHost();
    }

    private void NetworkManagerServerOnClientDisconnectCallback(ulong clientId)
    {
        for (var index = 0; index < playerDataNetworkList.Count; index++)
        {
            if (playerDataNetworkList[index].clientId != clientId) continue;
            playerDataNetworkList.RemoveAt(index);
            return;
        }
    }

    private void NetworkManagerOnClientConnectedCallback(ulong clientId)
    {
        playerDataNetworkList.Add(new PlayerData
        {
            clientId = clientId,
            colorId = GetFirstAvailableColorId(),
        });
        SetPlayerNameServerRpc(GetPlayerName());
    }

    private void NetworkManagerConnectionApprovalCallback(
        NetworkManager.ConnectionApprovalRequest connectionApprovalRequest,
        NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game has already started";
            return;
        }

        if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MaxPlayerAmount)
        {
            connectionApprovalResponse.Approved = false;
            connectionApprovalResponse.Reason = "Game is full";
            return;
        }

        connectionApprovalResponse.Approved = true;
    }

    public void StartClient()
    {
        OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManagerClientOnClientDisconnectCallback;
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManagerClientOnClientConnectedCallback;
        NetworkManager.Singleton.StartClient();
    }

    private void NetworkManagerClientOnClientConnectedCallback(ulong clientId)
    {
        SetPlayerNameServerRpc(GetPlayerName());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerNameServerRpc(string newPlayerName, ServerRpcParams serverRpcParams = default)
    {
        var playerIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        var playerData = playerDataNetworkList[playerIndex];
        playerData.playerName = newPlayerName;
        playerDataNetworkList[playerIndex] = playerData;
    }

    private void NetworkManagerClientOnClientDisconnectCallback(ulong obj)
    {
        OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
    }

    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSo, IKitchenObjectParent kitchenObjectParent)
    {
        SpawnKitchenObjectServerRpc(GetKitchenObjectSoIndex(kitchenObjectSo), kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectServerRpc(int kitchenObjectSoIndex,
        NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        var kitchenObjectSo = GetKitchenObjectSoFromIndex(kitchenObjectSoIndex);
        var kitchenObjectTransform = Instantiate(kitchenObjectSo.prefab);
        var networkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
        networkObject.Spawn(true);

        var kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
        kitchenObjectParentNetworkObjectReference.TryGet(out var kitchenObjectParentNetworkObject);
        var kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
        kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    public void DestroyKitchenObject(KitchenObject kitchenObject)
    {
        DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetObjReference)
    {
        kitchenObjectNetObjReference.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();

        ClearKitchenObjectParentClientRpc(kitchenObjectNetObjReference);
        kitchenObject.DestroySelf();
    }

    [ClientRpc]
    private void ClearKitchenObjectParentClientRpc(NetworkObjectReference kitchenObjectNetObjReference)
    {
        kitchenObjectNetObjReference.TryGet(out var kitchenObjectNetworkObject);
        var kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
        kitchenObject.ClearParent();
    }

    public int GetKitchenObjectSoIndex(KitchenObjectSO kitchenObjectSo)
    {
        return kitchenObjectListSo.kitchenObjectSoList.IndexOf(kitchenObjectSo);
    }

    public KitchenObjectSO GetKitchenObjectSoFromIndex(int index)
    {
        return kitchenObjectListSo.kitchenObjectSoList[index];
    }

    public bool IsPlayerIndexConnected(int index)
    {
        return index < playerDataNetworkList.Count;
    }

    public PlayerData GetPlayerData()
    {
        return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
    }

    public PlayerData GetPlayerDataFromClientId(ulong clientId)
    {
        foreach (var playerData in playerDataNetworkList)
        {
            if (playerData.clientId == clientId) return playerData;
        }

        return default;
    }

    public PlayerData GetPlayerDataFromPlayerIndex(int index)
    {
        return playerDataNetworkList[index];
    }

    public int GetPlayerIndexFromClientId(ulong clientId)
    {
        for (var index = 0; index < playerDataNetworkList.Count; index++)
        {
            if (playerDataNetworkList[index].clientId == clientId) return index;
        }

        return -1;
    }

    public Color GetColorById(int colorId)
    {
        return playerColorList[colorId];
    }

    public void SetPlayerColor(int colorId)
    {
        SetPlayerColorServerRpc(colorId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
    {
        if (!IsColorAvailable(colorId)) return;

        var playerIndex = GetPlayerIndexFromClientId(serverRpcParams.Receive.SenderClientId);
        var playerData = playerDataNetworkList[playerIndex];
        playerData.colorId = colorId;
        playerDataNetworkList[playerIndex] = playerData;
    }

    private bool IsColorAvailable(int colorId)
    {
        foreach (var playerData in playerDataNetworkList)
        {
            if (playerData.colorId == colorId) return false;
        }

        return true;
    }

    private int GetFirstAvailableColorId()
    {
        for (var id = 0; id < playerColorList.Count; id++)
        {
            if (IsColorAvailable(id)) return id;
        }

        return -1;
    }

    public void KickPlayer(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        NetworkManagerServerOnClientDisconnectCallback(clientId);
    }
}