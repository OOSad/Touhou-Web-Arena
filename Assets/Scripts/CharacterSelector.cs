using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System;

public class CharacterSelector : NetworkBehaviour
{
    [Header("Scene References")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI player1SelectionText;
    [SerializeField] private TextMeshProUGUI player2SelectionText;
    [SerializeField] private Button[] characterButtons;
    
    [Header("Character Options")]
    [SerializeField] private string[] characterNames = { "Hakurei Reimu", "Kirisame Marisa" };
    
    // Network variables to track player selections
    private NetworkVariable<int> player1Selection = new NetworkVariable<int>(-1, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private NetworkVariable<int> player2Selection = new NetworkVariable<int>(-1, 
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    // Dictionary to keep track of which client is which player
    private Dictionary<ulong, int> clientPlayerMap = new Dictionary<ulong, int>();
    
    // Current player number for local client (1 or 2)
    private int localPlayerNumber = 0;
    
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Assign player numbers to clients
            AssignPlayerNumbers();
            
            // Listen for client disconnections
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
        }
        
        // Register to selection change events
        player1Selection.OnValueChanged += OnPlayer1SelectionChanged;
        player2Selection.OnValueChanged += OnPlayer2SelectionChanged;
        
        // Setup button listeners
        SetupButtons();
        
        // Update UI with current selections
        UpdateSelectionUI();
    }
    
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
        
        player1Selection.OnValueChanged -= OnPlayer1SelectionChanged;
        player2Selection.OnValueChanged -= OnPlayer2SelectionChanged;
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
    
    [ClientRpc]
    private void NotifyPlayerNumberClientRpc(int playerNumber, ClientRpcParams clientRpcParams = default)
    {
        localPlayerNumber = playerNumber;
        Debug.Log($"You are Player {localPlayerNumber}");
    }
    
    private void HandleClientDisconnect(ulong clientId)
    {
        if (!IsServer) return;
        
        if (clientPlayerMap.TryGetValue(clientId, out int playerNumber))
        {
            Debug.Log($"Player {playerNumber} disconnected");
            
            // Reset selection for the disconnected player
            if (playerNumber == 1)
            {
                player1Selection.Value = -1;
            }
            else if (playerNumber == 2)
            {
                player2Selection.Value = -1;
            }
            
            clientPlayerMap.Remove(clientId);
        }
    }
    
    private void SetupButtons()
    {
        if (characterButtons == null || characterButtons.Length == 0)
        {
            Debug.LogError("Character buttons not assigned!");
            return;
        }
        
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int characterIndex = i; // Capture the index for the lambda
            
            if (characterButtons[i] != null)
            {
                // Remove any existing listeners to prevent duplicates
                characterButtons[i].onClick.RemoveAllListeners();
                
                // Add button click listener
                characterButtons[i].onClick.AddListener(() => SelectCharacter(characterIndex));
            }
        }
    }
    
    private void SelectCharacter(int characterIndex)
    {
        if (localPlayerNumber == 0)
        {
            Debug.LogError("Player number not assigned yet!");
            return;
        }
        
        if (characterIndex < 0 || characterIndex >= characterNames.Length)
        {
            Debug.LogError($"Invalid character index: {characterIndex}");
            return;
        }
        
        Debug.Log($"Player {localPlayerNumber} selected character: {characterNames[characterIndex]}");
        
        // Request server to update the selection
        SelectCharacterServerRpc(characterIndex, localPlayerNumber);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SelectCharacterServerRpc(int characterIndex, int playerNumber, ServerRpcParams serverRpcParams = default)
    {
        // Update the appropriate player's selection
        if (playerNumber == 1)
        {
            player1Selection.Value = characterIndex;
        }
        else if (playerNumber == 2)
        {
            player2Selection.Value = characterIndex;
        }
        
        // Check if both players have made selections
        CheckBothPlayersSelected();
    }
    
    private void OnPlayer1SelectionChanged(int previousValue, int newValue)
    {
        UpdateSelectionUI();
    }
    
    private void OnPlayer2SelectionChanged(int previousValue, int newValue)
    {
        UpdateSelectionUI();
    }
    
    private void UpdateSelectionUI()
    {
        if (player1SelectionText != null)
        {
            player1SelectionText.text = player1Selection.Value >= 0 ? 
                $"Player 1: {characterNames[player1Selection.Value]}" : "Player 1: Not Selected";
        }
        
        if (player2SelectionText != null)
        {
            player2SelectionText.text = player2Selection.Value >= 0 ? 
                $"Player 2: {characterNames[player2Selection.Value]}" : "Player 2: Not Selected";
        }
    }
    
    private void CheckBothPlayersSelected()
    {
        if (!IsServer) return;
        
        if (player1Selection.Value >= 0 && player2Selection.Value >= 0)
        {
            Debug.Log("Both players have selected characters! Loading gameplay scene...");
            
            // Wait a moment before loading the next scene to allow players to see their selections
            Invoke(nameof(LoadGameplayScene), 2.0f);
        }
    }
    
    private void LoadGameplayScene()
    {
        if (!IsServer) return;
        
        // Store selected characters somewhere before loading the scene
        // (This could be in a static class or PlayerPrefs for simplicity)
        PlayerSelections.Player1Character = characterNames[player1Selection.Value];
        PlayerSelections.Player2Character = characterNames[player2Selection.Value];
        
        // Load the gameplay scene for all clients
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
} 