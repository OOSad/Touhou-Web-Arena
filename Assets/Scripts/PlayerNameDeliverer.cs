using TMPro;
using UnityEngine;

public class PlayerNameDeliverer : MonoBehaviour
{
    public ClientInfo clientInfo;
    public TMP_InputField playerNameInputField;

    private void Update()
    {
        playerNameInputField.onEndEdit.AddListener(OnEndEditAction);
    }

    private void OnEndEditAction(string playerName)
    {
        clientInfo.playerName = playerName;
    }


}
