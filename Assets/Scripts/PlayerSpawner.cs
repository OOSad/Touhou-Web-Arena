using FishNet.Managing;
using FishNet.Managing.Server;
using GameKit.Dependencies.Utilities;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public ServerManager serverManager;
    public GameObject playerOne;
    public GameObject playerTwo;
    public Transform playerOneSpawn;
    public Transform playerTwoSpawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        serverManager = FindAnyObjectByType<NetworkManager>().GetComponent<ServerManager>();

        playerOne = Instantiate(playerOne);
        serverManager.Spawn(playerOne);
        playerOne.GetComponent<Transform>().SetPosition(true, playerOneSpawn.transform.position);
        playerOne.GetComponentInChildren<SpriteRenderer>().enabled = true;

        playerTwo = Instantiate(playerTwo);
        serverManager.Spawn(playerTwo);
        playerTwo.GetComponent<Transform>().SetPosition(true, playerTwoSpawn.transform.position);
        playerTwo.GetComponentInChildren<SpriteRenderer>().enabled = true;



    }

    // Update is called once per frame
    void Update()
    {
       
    }
}
