using Mirror;
using UnityEngine;

namespace Networking
{
    public class RTSNetworkManager : NetworkManager
    {
        [SerializeField] private GameObject unitSpawnerPrefab;

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);

            var addedPlayerTransform = conn.identity.transform;
            var unitSpawnerInstance = Instantiate(unitSpawnerPrefab, addedPlayerTransform.position,
                addedPlayerTransform.rotation);

            NetworkServer.Spawn(unitSpawnerInstance, conn);
        }
    }
}