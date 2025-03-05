using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;

public class StartClientButton : MonoBehaviour
{
    public NetworkManager networkManager;
    public Button startClientButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startClientButton.GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        networkManager.StartClient();
        Debug.Log("Client is connecting!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
