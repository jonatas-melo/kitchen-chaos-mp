using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitingForOtherPlayersUI : MonoBehaviour
{
    private void Start()
    {
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += GameManagerOnLocalPlayerReadyChanged;
        KitchenGameManager.Instance.OnStateChanged += GameManagerOnStateChanged;

        Hide();
    }

    private void GameManagerOnStateChanged(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsCountdownToStartActive()) return;
        Hide();
    }

    private void GameManagerOnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsLocalPlayerReady()) return;
        Show();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
}