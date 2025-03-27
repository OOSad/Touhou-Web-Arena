using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using UnityEngine;

public class ClientConnectionTracker : NetworkBehaviour
{
    [SerializeField] private string _sceneToLoad;
    private bool _sceneLoaded;

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkManager.ServerManager.OnRemoteConnectionState += HandleConnectionChange;
    }

    private void HandleConnectionChange(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        CheckClientCountAndLoadScene();
    }

    private void CheckClientCountAndLoadScene()
    {
        int clientsCount = NetworkManager.ServerManager.Clients.Count;

        if (clientsCount == 2 && !_sceneLoaded)
        {
            LoadNewScene();
            _sceneLoaded = true;
        }
        else if (clientsCount < 2)
        {
            _sceneLoaded = false;
        }
    }

    private void LoadNewScene()
    {
        SceneLoadData sceneLoadData = new SceneLoadData(_sceneToLoad);
        NetworkManager.SceneManager.LoadGlobalScenes(sceneLoadData);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        NetworkManager.ServerManager.OnRemoteConnectionState -= HandleConnectionChange;
    }
}