using UnityEngine;
using Unity.Netcode;

public class MultiplayerVehicleSpawner : NetworkBehaviour
{
    [Header("Vehicle Prefabs")]

    [SerializeField]
    private GameObject truckPrefab;

    [SerializeField]
    private GameObject carPrefab;


    [Header("Spawn Points")]

    [SerializeField]
    private Transform truckSpawnPoint;

    [SerializeField]
    private Transform carSpawnPoint;


    [Header("Lobby UI")]

    [SerializeField]
    private GameObject lobbyCanvas;


    private bool vehicleSpawned = false;


    // =========================================================
    // NETWORK START
    // =========================================================

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback +=
            OnClientConnected;

        // Host is already connected when the host starts.
        SpawnHostTruck();
    }


    // =========================================================
    // CLIENT CONNECTED
    // =========================================================

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log(
            "Player connected: " + clientId
        );

        // Ignore host.
        if (clientId == NetworkManager.Singleton.LocalClientId)
            return;

        // This is the second player.
        SpawnClientCar(clientId);
    }


    // =========================================================
    // HOST = TRUCK
    // =========================================================

    private void SpawnHostTruck()
    {
        if (!IsServer)
            return;

        if (truckPrefab == null)
        {
            Debug.LogError(
                "Truck Prefab is not assigned!"
            );

            return;
        }

        if (truckSpawnPoint == null)
        {
            Debug.LogError(
                "Truck Spawn Point is not assigned!"
            );

            return;
        }


        GameObject truck = Instantiate(
            truckPrefab,
            truckSpawnPoint.position,
            truckSpawnPoint.rotation
        );


        NetworkObject networkObject =
            truck.GetComponent<NetworkObject>();


        if (networkObject == null)
        {
            Debug.LogError(
                "Truck prefab needs a NetworkObject!"
            );

            Destroy(truck);

            return;
        }


        ulong hostId =
            NetworkManager.Singleton.LocalClientId;


        networkObject.SpawnWithOwnership(hostId);


        Debug.Log(
            "HOST VEHICLE SPAWNED: TRUCK"
        );


        CheckHideLobby();
    }


    // =========================================================
    // CLIENT = CAR
    // =========================================================

    private void SpawnClientCar(ulong clientId)
    {
        if (!IsServer)
            return;

        if (carPrefab == null)
        {
            Debug.LogError(
                "Car Prefab is not assigned!"
            );

            return;
        }

        if (carSpawnPoint == null)
        {
            Debug.LogError(
                "Car Spawn Point is not assigned!"
            );

            return;
        }


        GameObject car = Instantiate(
            carPrefab,
            carSpawnPoint.position,
            carSpawnPoint.rotation
        );


        NetworkObject networkObject =
            car.GetComponent<NetworkObject>();


        if (networkObject == null)
        {
            Debug.LogError(
                "Car prefab needs a NetworkObject!"
            );

            Destroy(car);

            return;
        }


        networkObject.SpawnWithOwnership(clientId);


        Debug.Log(
            "CLIENT VEHICLE SPAWNED: CAR"
        );


        CheckHideLobby();
    }


    // =========================================================
    // HIDE LOBBY
    // =========================================================

    private void CheckHideLobby()
    {
        if (!IsServer)
            return;


        int playerCount =
            NetworkManager.Singleton.ConnectedClients.Count;


        if (playerCount >= 2)
        {
            HideLobbyClientRpc();
        }
    }


    // =========================================================
    // HIDE UI ON ALL PLAYERS
    // =========================================================

    [ClientRpc]
    private void HideLobbyClientRpc()
    {
        if (lobbyCanvas != null)
        {
            lobbyCanvas.SetActive(false);
        }

        Debug.Log(
            "Lobby Canvas hidden. Race can begin."
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!IsServer)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -=
            OnClientConnected;
    }
}