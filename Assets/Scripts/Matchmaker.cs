using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using FishNet.Managing.Scened;
using Unity.VisualScripting;

public class Matchmaker : NetworkBehaviour
{
    public Button queueButton;
    public NetworkManager availablePlayer;
    public List<NetworkManager> availablePlayerPool = new List<NetworkManager>();
    public NetworkManager playerOne;
    public NetworkManager playerTwo;
    public SceneLoadData characterSelectScene = new SceneLoadData("CharacterSelectScene");

    private void Update()
    {
        if (playerOne && playerTwo)
        {
            NetworkManager.SceneManager.LoadGlobalScenes(characterSelectScene);
        }
    }
    private void Awake()
    {
        queueButton.onClick.AddListener(QueueButtonOnClick);
    }

    [ServerRpc(RequireOwnership = false)]
    public void QueueButtonOnClick()
    {
        availablePlayerPool.Add(availablePlayer);

        availablePlayerPool.Sort();

        if (availablePlayerPool.Count >= 2)
        {
            playerOne = availablePlayerPool[0];
            playerTwo = availablePlayerPool[1];

            availablePlayerPool.RemoveAt(0);
            availablePlayerPool.RemoveAt(0);
        }
    }
}
