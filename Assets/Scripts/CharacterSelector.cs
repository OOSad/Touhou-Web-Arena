using FishNet.Connection;
using FishNet.Managing;
using FishNet.Managing.Server;
using FishNet.Object;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : NetworkBehaviour
{
    public Matchmaker matchmaker;
    public Button reimuButton;
    public Button marisaButton;
    public NetworkManager playerOne;
    public NetworkManager playerTwo;
    public TextMeshProUGUI playerOneSelectedCharacterText;
    public TextMeshProUGUI playerTwoSelectedCharacterText;

    private void Awake()
    {
        reimuButton.onClick.AddListener(ReimuButtonOnClick);
        marisaButton.onClick.AddListener(MarisaButtonOnClick);
        matchmaker = GameObject.Find("Matchmaker").GetComponent<Matchmaker>();
    }

    [ServerRpc (RequireOwnership = false)]
    public void ReimuButtonOnClick()
    {
        //Debug.Log("Player One ID: " + NetworkConnection);
        Debug.Log("Player Two ID: " + LocalConnection);


        if (LocalConnection.ClientId == matchmaker.playerOne.ClientManager.Connection.ClientId)
        {
            playerOneSelectedCharacterText.text = "Hakurei Reimu";
        }

        else if (LocalConnection.ClientId == matchmaker.playerTwo.ClientManager.Connection.ClientId)
        {
            playerTwoSelectedCharacterText.text = "Hakurei Reimu";
        }
    }

    public void MarisaButtonOnClick()
    {

    }

}
