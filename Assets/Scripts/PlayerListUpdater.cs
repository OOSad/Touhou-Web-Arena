using FishNet.Object.Synchronizing;
using FishNet.Transporting.Bayou;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListUpdater : MonoBehaviour
{
    public Bayou bayou;
    public TextMeshProUGUI playerList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        playerList.text = bayou.NetworkManager.ServerManager.Clients.Count.ToString();
    }
}
