using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class ServerStarter : MonoBehaviour
{
    [SerializeField] private Button startServerButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (startServerButton != null)
        {
            startServerButton.onClick.AddListener(StartServer);
        }
        else
        {
            Debug.LogError("Start Server Button not assigned in the inspector!");
        }
    }

    // Method to start the server
    public void StartServer()
    {
        if (NetworkManager.Singleton.StartServer())
        {
            Debug.Log("Server started successfully!");
        }
        else
        {
            Debug.LogError("Failed to start server!");
        }
    }
}
