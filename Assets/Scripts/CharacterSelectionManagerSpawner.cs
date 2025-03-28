using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Server;

public class CharacterSelectionManagerSpawner : NetworkBehaviour
{
    public GameObject characterSelectionManagerPrefab;

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        // If this is a client, request a character selection manager
        if (!IsServer && IsClient)
        {
            Debug.Log("Client requesting character spawn");
            RequestCharacterSpawnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCharacterSpawnServerRpc()
    {
        // Get the connection from the client that called this method
        NetworkConnection conn = Owner;
        
        Debug.Log("Server received request to spawn character selection manager");
        
        // Spawn the character
        GameObject characterManagerInstance = Instantiate(characterSelectionManagerPrefab);
        NetworkObject networkObject = characterManagerInstance.GetComponent<NetworkObject>();
        
        // Use the NetworkServer to spawn the object
        ServerManager.Spawn(characterManagerInstance, conn);
        
        Debug.Log("Character spawned and ownership assigned to connection: " + conn);
    }
} 