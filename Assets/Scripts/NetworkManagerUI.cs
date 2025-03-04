using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManagerUI : MonoBehaviour
{
    [SerializeField] private Button serverButton;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TextMeshProUGUI playerList;

    private void Update()
    {
        
    }

     private void Awake()
    {
        serverButton.onClick.AddListener(() =>
        {
            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("ltd-appendix.gl.at.ply.gg", (ushort)40476);
            NetworkManager.Singleton.StartServer();
        });

        clientButton.onClick.AddListener(() =>
        {
            //NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("ltd-appendix.gl.at.ply.gg", (ushort)40476);
            NetworkManager.Singleton.StartClient();
            playerList.text = NetworkManager.Singleton.ConnectedClients.ToString();
        });
    }
    
}
