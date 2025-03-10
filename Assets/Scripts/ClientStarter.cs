using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using UnityEngine;

public class ClientStarter : MonoBehaviour
{
    NetworkManager networkManager;

    private void Start()
    {
        networkManager = gameObject.GetComponent<NetworkManager>();
    }

    public void StartClient()
    {
        networkManager.StartClient();
    }
}
