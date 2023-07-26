using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectColorSingleUI : MonoBehaviour
{
    [SerializeField] private int colorId;
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectedGameObject;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            KitchenObjectMultiplayer.Instance.SetPlayerColor(colorId);
        });
    }

    private void Start()
    {
        KitchenObjectMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayerOnPlayerDataNetworkListChanged;
        image.color = KitchenObjectMultiplayer.Instance.GetColorById(colorId);
        UpdateIsSelected();
    }

    private void GameMultiplayerOnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdateIsSelected();
    }

    private void UpdateIsSelected()
    {
        var playerData = KitchenObjectMultiplayer.Instance.GetPlayerData();
        selectedGameObject.SetActive(colorId == playerData.colorId);
    }

    private void OnDestroy()
    {
        KitchenObjectMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayerOnPlayerDataNetworkListChanged;
    }
}