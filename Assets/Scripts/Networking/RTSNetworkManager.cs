using Buildings;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class RTSNetworkManager : NetworkManager
    {
        [SerializeField] private GameObject unitSpawnerPrefab;
        [SerializeField] private GameOverHandler gameOverHandlerPrefab;

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);

            var player = conn.identity.GetComponent<RTSPlayer>();
            player.SetTeamColor(new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
                ));

            var addedPlayerTransform = conn.identity.transform;
            var unitSpawnerInstance = Instantiate(unitSpawnerPrefab, addedPlayerTransform.position,
                addedPlayerTransform.rotation);

            NetworkServer.Spawn(unitSpawnerInstance, conn);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
            {
                var gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);
                
                NetworkServer.Spawn(gameOverHandlerInstance.gameObject);
            }
        }
    }
}