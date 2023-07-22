using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ContainerCounter : BaseCounter
{
    public event EventHandler OnPlayerGrabbedObject;


    [SerializeField] private KitchenObjectSO kitchenObjectSO;


    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            // Player is not carrying anything
            KitchenObject.SpawnKitchenObject(kitchenObjectSO, player);
            GrabbedObjectLogicServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void GrabbedObjectLogicServerRpc()
    {
        GrabbedObjectLogicClientRpc();
    }

    [ClientRpc]
    private void GrabbedObjectLogicClientRpc()
    {
        OnPlayerGrabbedObject?.Invoke(this, EventArgs.Empty);
    }
}