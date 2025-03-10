using Unity.Netcode;
using UnityEngine;

public class ServerStarter : MonoBehaviour
{
    NetworkManager networkManager;

    private void Start()
    {
        networkManager = gameObject.GetComponent<NetworkManager>();
    }

    public void StartServer()
    {
        networkManager.StartServer();
    }

    
}
