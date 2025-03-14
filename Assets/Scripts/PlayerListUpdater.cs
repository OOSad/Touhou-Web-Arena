using FishNet.Object;
using FishNet.Object.Synchronizing;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
using FishNet.Connection;
using JetBrains.Annotations;
using UnityEngine.UI;

public class PlayerListUpdater : NetworkBehaviour
{
    public ClientInfo clientInfo;
    public Button queueButton;
    public Button cancelButton;
    public TextMeshProUGUI physicalListOfPlayers;
    public readonly SyncList<string> listOfPlayers = new SyncList<string> ();

    private void Awake()
    {
        queueButton.onClick.AddListener(QueueButtonOnClick);
        cancelButton.onClick.AddListener(CancelButtonOnClick);
    }

    private void Update()
    {
        physicalListOfPlayers.text = "";
        for (int i = 0; i < listOfPlayers.Count; i++)
        {
            physicalListOfPlayers.text += listOfPlayers[i];
            //Debug.Log(listOfPlayers[i]);
        }
    }

    public void QueueButtonOnClick()
    {
        base.OnStartClient();
        AddPlayerUsername(clientInfo.playerName);
        Debug.Log("Started client.");

    }

    public void CancelButtonOnClick()
    {
        base.OnStopClient();
        RemovePlayerUsername(clientInfo.playerName);
        Debug.Log("Stopped client.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerUsername(string username)
    {
        if (!listOfPlayers.Contains(username))
        {
            listOfPlayers.Add(username);
        }
        Debug.Log("Wrote to player list.");
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerUsername(string username)
    {
        listOfPlayers.Remove(username);
        Debug.Log("Removed from player list.");
    }
}
