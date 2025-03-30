using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : NetworkBehaviour
{
    [System.Serializable]
    public class CharacterPrefabMapping
    {
        public string characterName;
        public GameObject prefab;
    }

    [Header("Character Prefabs")]
    [SerializeField] private List<CharacterPrefabMapping> characterPrefabs = new List<CharacterPrefabMapping>();

    [Header("Spawn Points")]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    // Dictionary to keep track of which client is which player
    private Dictionary<ulong, int> clientPlayerMap = new Dictionary<ulong, int>();
    
    // Keep track of spawned player objects
    private Dictionary<int, NetworkObject> spawnedPlayers = new Dictionary<int, NetworkObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // We're in the gameplay scene now, assign player numbers and spawn characters
            AssignPlayerNumbers();
            SpawnPlayers();
            
            // Listen for disconnections
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }

    private void AssignPlayerNumbers()
    {
        if (!IsServer) return;

        int playerNumber = 1;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            clientPlayerMap[clientId] = playerNumber;
            NotifyPlayerNumberClientRpc(playerNumber, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            });

            playerNumber++;

            // Only support 2 players for now
            if (playerNumber > 2) break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void GetPlayerNumberServerRpc(ulong clientId, ServerRpcParams serverRpcParams = default)
    {
        // Get the requesting client's ID
        ulong requestingClientId = serverRpcParams.Receive.SenderClientId;
        
        // Find the player number for this client
        int playerNumber = 0;
        if (clientPlayerMap.TryGetValue(clientId, out playerNumber))
        {
            // Found the player number, notify the client
            NotifyPlayerNumberClientRpc(playerNumber, new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { requestingClientId }
                }
            });
            
            Debug.Log($"Server: Client {clientId} is Player {playerNumber}");
        }
        else
        {
            Debug.LogWarning($"Server: Could not find player number for client {clientId}");
        }
    }

    [ClientRpc]
    private void NotifyPlayerNumberClientRpc(int playerNumber, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log($"Client: Received player number {playerNumber}");
        
        // Find all PlayerController instances belonging to this client using non-deprecated method
        PlayerController[] controllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (PlayerController controller in controllers)
        {
            // Check if this controller belongs to the local player
            if (controller.IsOwner)
            {
                controller.SetPlayerNumber(playerNumber);
            }
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        if (!IsServer) return;

        if (clientPlayerMap.TryGetValue(clientId, out int playerNumber))
        {
            Debug.Log($"Player {playerNumber} disconnected");

            // Clean up the spawned player if needed
            if (spawnedPlayers.TryGetValue(playerNumber, out NetworkObject playerObject))
            {
                if (playerObject != null && playerObject.IsSpawned)
                {
                    playerObject.Despawn();
                }
                spawnedPlayers.Remove(playerNumber);
            }

            clientPlayerMap.Remove(clientId);
        }
    }

    private void SpawnPlayers()
    {
        if (!IsServer) return;

        string player1Character = PlayerSelections.Player1Character;
        string player2Character = PlayerSelections.Player2Character;

        Debug.Log($"Spawning Player 1 as {player1Character}");
        Debug.Log($"Spawning Player 2 as {player2Character}");

        // Validate spawn points
        if (player1SpawnPoint == null || player2SpawnPoint == null)
        {
            Debug.LogError("Spawn points not set!");
            return;
        }

        // Find client IDs for player 1 and 2
        ulong? player1ClientId = GetClientIdForPlayerNumber(1);
        ulong? player2ClientId = GetClientIdForPlayerNumber(2);

        if (!player1ClientId.HasValue || !player2ClientId.HasValue)
        {
            Debug.LogError("Cannot find client IDs for both players!");
            return;
        }

        // Spawn Player 1's character
        GameObject player1Prefab = GetPrefabForCharacter(player1Character);
        if (player1Prefab != null)
        {
            NetworkObject player1Object = SpawnPlayerCharacter(player1Prefab, player1SpawnPoint, player1ClientId.Value);
            if (player1Object != null)
            {
                spawnedPlayers[1] = player1Object;
            }
        }

        // Spawn Player 2's character
        GameObject player2Prefab = GetPrefabForCharacter(player2Character);
        if (player2Prefab != null)
        {
            NetworkObject player2Object = SpawnPlayerCharacter(player2Prefab, player2SpawnPoint, player2ClientId.Value);
            if (player2Object != null)
            {
                spawnedPlayers[2] = player2Object;
                Debug.Log($"Player 2 spawned at position: {player2Object.gameObject.transform.position}");
            }
        }
    }

    private ulong? GetClientIdForPlayerNumber(int playerNumber)
    {
        foreach (var kvp in clientPlayerMap)
        {
            if (kvp.Value == playerNumber)
            {
                return kvp.Key;
            }
        }
        return null;
    }

    private GameObject GetPrefabForCharacter(string characterName)
    {
        foreach (var mapping in characterPrefabs)
        {
            if (mapping.characterName == characterName && mapping.prefab != null)
            {
                return mapping.prefab;
            }
        }

        Debug.LogError($"No prefab found for character: {characterName}");
        return null;
    }

    private NetworkObject SpawnPlayerCharacter(GameObject prefab, Transform spawnPoint, ulong ownerClientId)
    {
        // Instantiate the player character at the spawn point
        GameObject playerInstance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Debug.LogError("Player prefab does not have a NetworkObject component!");
            Destroy(playerInstance);
            return null;
        }

        // Spawn the network object with ownership assigned to the correct client
        networkObject.SpawnWithOwnership(ownerClientId);
        
        return networkObject;
    }
} 