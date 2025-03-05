using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListUpdater : MonoBehaviour
{
    public NetworkManager networkManager;
    public Button startClientButton;
    public TextMeshProUGUI playerList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startClientButton.GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        for (int i = 0; i < networkManager.ConnectedClients.Count; i++)

        playerList.text = networkManager.ConnectedClientsList[i].ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
