using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Text;
using Unity.Collections;  // Add this for FixedString64Bytes
using System;

public class Matchmaker : NetworkBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private int requiredPlayerCount = 2;
    [SerializeField] private float matchStartDelay = 3.0f;

    [Header("UI References")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button queueButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI queueText;

    private bool matchStarted = false;
    private Dictionary<ulong, bool> connectedClients = new Dictionary<ulong, bool>();
    
    // Network variable to track the number of connected players
    private NetworkVariable<int> connectedPlayerCount = new NetworkVariable<int>(0, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Network variable structure for player queue entries
    public struct PlayerQueueData : INetworkSerializable, IEquatable<PlayerQueueData>
    {
        public ulong ClientId;
        public FixedString64Bytes PlayerName;
        public bool IsQueued;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref IsQueued);
        }

        public bool Equals(PlayerQueueData other)
        {
            return ClientId == other.ClientId && 
                   PlayerName.Equals(other.PlayerName) && 
                   IsQueued == other.IsQueued;
        }

        public override bool Equals(object obj)
        {
            return obj is PlayerQueueData data && Equals(data);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ClientId, PlayerName, IsQueued);
        }
    }

    // List of queued players to be synced across the network
    private NetworkList<PlayerQueueData> queuedPlayers;

    private void Awake()
    {
        // Ensure we have a valid scene name
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("Game Scene Name is not set in the Matchmaker!");
        }

        // Initialize the NetworkList
        queuedPlayers = new NetworkList<PlayerQueueData>();
    }

    private void Start()
    {
        // Set up UI button listeners
        if (queueButton != null)
        {
            queueButton.onClick.AddListener(OnQueueButtonClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
            // Initially disable the cancel button
            cancelButton.interactable = false;
        }

        // Set a default player name if the input field is empty
        if (nameInputField != null && string.IsNullOrEmpty(nameInputField.text))
        {
            nameInputField.text = "Player" + UnityEngine.Random.Range(1000, 9999);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Register to connection events
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            
            // Count the server as a client if it's also a host
            if (IsHost)
            {
                HandleClientConnected(NetworkManager.Singleton.LocalClientId);
            }
            
            // Update for any clients that connected before we initialized
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if (!connectedClients.ContainsKey(clientId))
                {
                    HandleClientConnected(clientId);
                }
            }
        }

        // Register to the network variable changes
        connectedPlayerCount.OnValueChanged += OnPlayerCountChanged;
        queuedPlayers.OnListChanged += OnQueueChanged;

        // Update the queue display initially
        UpdateQueueDisplay();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            // Unregister from connection events
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        // Unregister from the network variable change
        connectedPlayerCount.OnValueChanged -= OnPlayerCountChanged;
        queuedPlayers.OnListChanged -= OnQueueChanged;

        // Clean up button listeners
        if (queueButton != null)
        {
            queueButton.onClick.RemoveListener(OnQueueButtonClicked);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(OnCancelButtonClicked);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        HandleClientConnected(clientId);
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (IsServer && !connectedClients.ContainsKey(clientId))
        {
            // Add the new client to our dictionary
            connectedClients[clientId] = true;
            
            // Update the connected player count
            connectedPlayerCount.Value = connectedClients.Count;
            
            Debug.Log($"Client connected: {clientId}. Total players: {connectedPlayerCount.Value}");
            
            // Add them to the queue list (not queued yet)
            PlayerQueueData newPlayer = new PlayerQueueData
            {
                ClientId = clientId,
                PlayerName = new FixedString64Bytes("Player" + clientId),
                IsQueued = false
            };
            
            queuedPlayers.Add(newPlayer);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer && connectedClients.ContainsKey(clientId))
        {
            // Remove the disconnected client
            connectedClients.Remove(clientId);
            
            // Update the connected player count
            connectedPlayerCount.Value = connectedClients.Count;
            
            Debug.Log($"Client disconnected: {clientId}. Total players: {connectedPlayerCount.Value}");
            
            // Remove them from the queue
            RemovePlayerFromQueue(clientId);
            
            // If match hasn't started yet, we may need to cancel countdown
            if (!matchStarted && !AreRequiredPlayersQueued())
            {
                Debug.Log("Not enough queued players. Waiting for more...");
                CancelInvoke(nameof(StartMatch));
            }
        }
    }

    private void OnPlayerCountChanged(int previousValue, int newValue)
    {
        // This runs on both server and client when the player count changes
        Debug.Log($"Player count changed from {previousValue} to {newValue}");
    }

    private void OnQueueChanged(NetworkListEvent<PlayerQueueData> changeEvent)
    {
        // Update the queue display whenever the queue changes
        UpdateQueueDisplay();
        
        // If we're the server, check if we should start the match
        if (IsServer)
        {
            CheckMatchStart();
        }
        
        // Update the UI based on our own queue status
        UpdateLocalPlayerUI();
    }

    private void UpdateQueueDisplay()
    {
        if (queueText == null) return;

        StringBuilder queueString = new StringBuilder("Players in Queue:\n");
        
        int queuedCount = 0;
        foreach (PlayerQueueData player in queuedPlayers)
        {
            if (player.IsQueued)
            {
                queuedCount++;
                queueString.AppendLine($"{queuedCount}. {player.PlayerName}");
            }
        }
        
        if (queuedCount == 0)
        {
            queueString.AppendLine("No players queued");
        }
        
        queueText.text = queueString.ToString();
    }

    private void OnQueueButtonClicked()
    {
        string playerName = nameInputField != null ? nameInputField.text : "Anonymous";
        
        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = "Player" + UnityEngine.Random.Range(1000, 9999);
            if (nameInputField != null)
            {
                nameInputField.text = playerName;
            }
        }
        
        // Request to queue from the server
        QueuePlayerServerRpc(playerName);
    }

    private void OnCancelButtonClicked()
    {
        // Request to cancel from the server
        CancelQueueServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void QueuePlayerServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        
        // Find the player in the list
        for (int i = 0; i < queuedPlayers.Count; i++)
        {
            PlayerQueueData player = queuedPlayers[i];
            if (player.ClientId == clientId)
            {
                // Update the player data
                player.PlayerName = new FixedString64Bytes(playerName);
                player.IsQueued = true;
                queuedPlayers[i] = player;
                
                Debug.Log($"Player {playerName} (ID: {clientId}) queued for match");
                
                // Check if we should start the match
                CheckMatchStart();
                return;
            }
        }
        
        // If player wasn't found in the list yet, add them
        PlayerQueueData newPlayer = new PlayerQueueData
        {
            ClientId = clientId,
            PlayerName = new FixedString64Bytes(playerName),
            IsQueued = true
        };
        
        queuedPlayers.Add(newPlayer);
        Debug.Log($"Player {playerName} (ID: {clientId}) queued for match");
        
        // Check if we should start the match
        CheckMatchStart();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CancelQueueServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        
        // Find the player in the list and update their queue status
        for (int i = 0; i < queuedPlayers.Count; i++)
        {
            PlayerQueueData player = queuedPlayers[i];
            if (player.ClientId == clientId)
            {
                player.IsQueued = false;
                queuedPlayers[i] = player;
                
                Debug.Log($"Player {player.PlayerName} (ID: {clientId}) canceled queue");
                
                // If match was about to start, cancel it
                if (!AreRequiredPlayersQueued())
                {
                    Debug.Log("Not enough queued players. Waiting for more...");
                    CancelInvoke(nameof(StartMatch));
                }
                
                return;
            }
        }
    }

    private void RemovePlayerFromQueue(ulong clientId)
    {
        // Find and remove the player from the queue list
        for (int i = 0; i < queuedPlayers.Count; i++)
        {
            if (queuedPlayers[i].ClientId == clientId)
            {
                queuedPlayers.RemoveAt(i);
                return;
            }
        }
    }

    private void UpdateLocalPlayerUI()
    {
        if (!IsSpawned) return;
        
        ulong localClientId = NetworkManager.Singleton.LocalClientId;
        bool isLocalPlayerQueued = false;
        
        // Check if the local player is queued
        foreach (PlayerQueueData player in queuedPlayers)
        {
            if (player.ClientId == localClientId)
            {
                isLocalPlayerQueued = player.IsQueued;
                break;
            }
        }
        
        // Enable/disable buttons based on queue status
        if (queueButton != null)
        {
            queueButton.interactable = !isLocalPlayerQueued;
        }
        
        if (cancelButton != null)
        {
            cancelButton.interactable = isLocalPlayerQueued;
        }
        
        if (nameInputField != null)
        {
            nameInputField.interactable = !isLocalPlayerQueued;
        }
    }

    private bool AreRequiredPlayersQueued()
    {
        if (connectedPlayerCount.Value < requiredPlayerCount)
        {
            return false;
        }
        
        int queuedCount = 0;
        foreach (PlayerQueueData player in queuedPlayers)
        {
            if (player.IsQueued)
            {
                queuedCount++;
            }
        }
        
        return queuedCount >= requiredPlayerCount;
    }

    private void CheckMatchStart()
    {
        if (!IsServer) return;
        
        if (!matchStarted && AreRequiredPlayersQueued())
        {
            Debug.Log($"Required queued players reached. Starting match in {matchStartDelay} seconds.");
            
            // Start the match after a delay
            Invoke(nameof(StartMatch), matchStartDelay);
        }
    }

    private void StartMatch()
    {
        if (!IsServer) return;
        
        if (!matchStarted && AreRequiredPlayersQueued())
        {
            matchStarted = true;
            Debug.Log("Starting match!");
            
            // Store the selected player names for use in the game scene
            int playerIndex = 1;
            foreach (PlayerQueueData player in queuedPlayers)
            {
                if (player.IsQueued && playerIndex <= 2)
                {
                    if (playerIndex == 1)
                    {
                        PlayerSelections.Player1Name = player.PlayerName.ToString();
                    }
                    else if (playerIndex == 2)
                    {
                        PlayerSelections.Player2Name = player.PlayerName.ToString();
                    }
                    playerIndex++;
                }
            }
            
            // Load the game scene for all clients
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }
} 