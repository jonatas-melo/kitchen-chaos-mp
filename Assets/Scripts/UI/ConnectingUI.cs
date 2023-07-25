using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectingUI : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("ConnectingUI Start");
        KitchenObjectMultiplayer.Instance.OnTryingToJoinGame += GameMultiplayerTryingToJoinGame;
        KitchenObjectMultiplayer.Instance.OnFailedToJoinGame += GameMultiplayerOnFailedToJoinGame;
        Hide();
    }

    private void GameMultiplayerTryingToJoinGame(object sender, EventArgs e)
    {
        Show();
    }

    private void GameMultiplayerOnFailedToJoinGame(object sender, EventArgs e)
    {
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
        KitchenObjectMultiplayer.Instance.OnFailedToJoinGame -= GameMultiplayerOnFailedToJoinGame;
        KitchenObjectMultiplayer.Instance.OnTryingToJoinGame -= GameMultiplayerTryingToJoinGame;
    }
}