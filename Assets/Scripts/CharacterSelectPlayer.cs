using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyTextGameObject;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickButton;
    [SerializeField] private TextMeshPro playerNameText;

    private void Awake()
    {
        kickButton.onClick.AddListener(() =>
        {
            var playerData = KitchenObjectMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            KitchenObjectMultiplayer.Instance.KickPlayer(playerData.clientId);
        });
    }

    private void Start()
    {
        KitchenObjectMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayerOnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnPlayerReadyChanged += CharacterSelectReadyOnPlayerReadyChanged;

        kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
        UpdatePlayer();
    }

    private void CharacterSelectReadyOnPlayerReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void GameMultiplayerOnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (KitchenObjectMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();
            var playerData = KitchenObjectMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            readyTextGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));
            playerNameText.text = playerData.playerName.ToString();
            playerVisual.SetColor(KitchenObjectMultiplayer.Instance.GetColorById(playerData.colorId));
            return;
        }

        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        KitchenObjectMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayerOnPlayerDataNetworkListChanged;
    }
}