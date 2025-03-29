using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ClientConnector : MonoBehaviour
{
    [SerializeField] private Button connectButton;

    void Start()
    {
        if (connectButton != null)
        {
            connectButton.onClick.AddListener(ConnectToServer);
        }
        else
        {
            Debug.LogError("Connect Button not assigned in the inspector!");
        }
    }

    public void ConnectToServer()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Client connected successfully!");
        }
        else
        {
            Debug.LogError("Failed to connect client!");
        }
    }
} 