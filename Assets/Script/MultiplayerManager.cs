using UnityEngine;
using Unity.Netcode;

public class MultiplayerManager : MonoBehaviour
{
    public void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Network is already running.");
            return;
        }

        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("HOST STARTED");
        }
        else
        {
            Debug.LogError("Failed to start Host.");
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("Network is already running.");
            return;
        }

        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("CLIENT STARTED");
        }
        else
        {
            Debug.LogError("Failed to start Client.");
        }
    }

    public void StopNetwork()
    {
        if (NetworkManager.Singleton != null &&
            NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();

            Debug.Log("NETWORK STOPPED");
        }
    }
}