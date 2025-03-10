using Unity.Netcode;
using UnityEngine;

public class NumberOfClientsDisplayer : MonoBehaviour
{
    public NetworkManager networkManager;

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < networkManager.ConnectedClientsList.Count; i++)
        {
            Debug.Log(networkManager.ConnectedClientsList[i].ToString());
        }
    }
}
