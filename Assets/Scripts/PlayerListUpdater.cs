using FishNet.Transporting.Bayou;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListUpdater : MonoBehaviour
{
    public Button refreshClientsButton;
    public Bayou bayou;
    public TextMeshProUGUI playerList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        refreshClientsButton.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        for (int i = 0; i < bayou.NetworkManager.ClientManager.Clients.Count; i++)
        {
            playerList.text = bayou.NetworkManager.ClientManager.Clients[i].ToString();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
