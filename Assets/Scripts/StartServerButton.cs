using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;

public class StartServerButton : MonoBehaviour
{
    public NetworkManager networkManager;
    public Button startServerButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startServerButton.GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        networkManager.StartServer();
        Debug.Log("Server is started!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
