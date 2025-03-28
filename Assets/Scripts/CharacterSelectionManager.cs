using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Managing.Scened;
using System.Collections;

public class CharacterSelectionManager : NetworkBehaviour
{
    [SerializeField] private Button characterButton1;
    [SerializeField] private Button characterButton2;
    [SerializeField] private TextMeshProUGUI player1SelectionText;
    [SerializeField] private TextMeshProUGUI player2SelectionText;
    [SerializeField] private string nextSceneName = "StageScene";
    [SerializeField] private float sceneLoadDelay = 2f;

    private string player1Selection = "";
    private string player2Selection = "";
    private NetworkConnection player1Connection;
    private NetworkConnection player2Connection;
    private int currentPlayerSelection = 0;

    public override void OnStartClient()
    {
        base.OnStartClient();
        FindAndSetupUIElements();
    }

    private void FindAndSetupUIElements()
    {
        // Find UI elements by name if not assigned in inspector
        characterButton1 ??= GameObject.Find("HakureiReimuButton")?.GetComponent<Button>();
        characterButton2 ??= GameObject.Find("KirisameMarisaButton")?.GetComponent<Button>();
        player1SelectionText ??= GameObject.Find("PlayerOneCharacterText")?.GetComponent<TextMeshProUGUI>();
        player2SelectionText ??= GameObject.Find("PlayerTwoCharacterText")?.GetComponent<TextMeshProUGUI>();

        // Set up button listeners
        if (characterButton1 != null)
            characterButton1.onClick.AddListener(() => CmdSelectCharacterServerRpc(1, "Hakurei Reimu"));

        if (characterButton2 != null)
            characterButton2.onClick.AddListener(() => CmdSelectCharacterServerRpc(2, "Kirisame Marisa"));
    }

    private void CheckSelections()
    {
        if (!string.IsNullOrEmpty(player1Selection) &&
            !string.IsNullOrEmpty(player2Selection) &&
            player1Connection != null &&
            player2Connection != null &&
            player1Connection != player2Connection)
        {
            StartCoroutine(DelayedSceneLoad());
        }
    }

    private IEnumerator DelayedSceneLoad()
    {
        yield return new WaitForSeconds(sceneLoadDelay);

        SceneLoadData sceneLoadData = new SceneLoadData(nextSceneName);
        SceneManager.LoadGlobalScenes(sceneLoadData);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CmdSelectCharacterServerRpc(int playerNumber, string characterName, NetworkConnection conn = null)
    {
        int assignedPlayerSlot = DeterminePlayerSlot(conn);

        if (assignedPlayerSlot == 0)
        {
            Debug.LogError("No available player slots");
            return;
        }

        AssignPlayerConnection(assignedPlayerSlot, conn);
        UpdatePlayerSelection(assignedPlayerSlot, characterName);

        UpdateClientSelectionTrackerTargetRpc(conn, playerNumber);

        CheckSelections();
    }

    private int DeterminePlayerSlot(NetworkConnection conn)
    {
        if (conn == player1Connection) return 1;
        if (conn == player2Connection) return 2;
        if (player1Connection == null) return 1;
        if (player2Connection == null) return 2;
        return 0;
    }

    private void AssignPlayerConnection(int assignedPlayerSlot, NetworkConnection conn)
    {
        if (assignedPlayerSlot == 1 && player1Connection == null)
            player1Connection = conn;
        else if (assignedPlayerSlot == 2 && player2Connection == null)
            player2Connection = conn;
    }

    private void UpdatePlayerSelection(int assignedPlayerSlot, string characterName)
    {
        if (assignedPlayerSlot == 1)
        {
            player1Selection = characterName;
            UpdateSelectionObserversRpc(1, characterName);
        }
        else
        {
            player2Selection = characterName;
            UpdateSelectionObserversRpc(2, characterName);
        }
    }

    [ObserversRpc]
    private void UpdateSelectionObserversRpc(int playerNumber, string characterName)
    {
        if (playerNumber == 1 && player1SelectionText != null)
            player1SelectionText.text = "Player 1: " + characterName;
        else if (playerNumber == 2 && player2SelectionText != null)
            player2SelectionText.text = "Player 2: " + characterName;
    }

    [TargetRpc]
    private void UpdateClientSelectionTrackerTargetRpc(NetworkConnection target, int playerNumber)
    {
        currentPlayerSelection = playerNumber;
        UpdateButtonVisuals();
    }

    private void UpdateButtonVisuals()
    {
        if (characterButton1 != null)
        {
            ColorBlock colors = characterButton1.colors;
            colors.normalColor = currentPlayerSelection == 1 ? new Color(0.5f, 1f, 0.5f) : Color.white;
            characterButton1.colors = colors;
        }

        if (characterButton2 != null)
        {
            ColorBlock colors = characterButton2.colors;
            colors.normalColor = currentPlayerSelection == 2 ? new Color(0.5f, 1f, 0.5f) : Color.white;
            characterButton2.colors = colors;
        }
    }
}