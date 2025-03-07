using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting.Bayou;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListUpdater : NetworkBehaviour
{
    public Bayou bayou;
    
    public TextMeshProUGUI playerList;
    public readonly SyncVar<int> playerNumber = new SyncVar<int>(); 

    // Update is called once per frame
    void Update()
    {
        playerNumber.Value = bayou.NetworkManager.ServerManager.Clients.Count;
        playerList.text = playerNumber.Value.ToString();
    }
}
