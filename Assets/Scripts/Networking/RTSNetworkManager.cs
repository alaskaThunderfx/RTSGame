using System;
using System.Collections.Generic;
using Buildings;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Networking
{
    public class RTSNetworkManager : NetworkManager
    {
        [SerializeField] private GameObject unitBasePrefab;
        [SerializeField] private GameOverHandler gameOverHandlerPrefab;

        public static event Action ClientOnConnected;
        public static event Action ClientOnDisconnected;

        private bool _isGameInProgress = false;

        public List<RTSPlayer> Players { get; } = new();

        #region Server

        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            if (!_isGameInProgress) return;

            conn.Disconnect();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            var player = conn.identity.GetComponent<RTSPlayer>();
            
            Players.Remove(player);
            
            base.OnServerDisconnect(conn);
        }

        public override void OnStopServer()
        {
            Players.Clear();

            _isGameInProgress = false;
        }

        public void StartGame()
        {
            if (Players.Count < 2) return;

            _isGameInProgress = true;
            
            ServerChangeScene("Scene_Map_01");
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);

            var player = conn.identity.GetComponent<RTSPlayer>();
            
            Players.Add(player);
            
            player.SetTeamColor(new Color(
                Random.Range(0f, 1f),
                Random.Range(0f, 1f),
                Random.Range(0f, 1f)
            ));
            
            player.SetPartyOwner(Players.Count == 1);

            // var addedPlayerTransform = conn.identity.transform;
            // var unitSpawnerInstance = Instantiate(unitSpawnerPrefab, addedPlayerTransform.position,
            //     addedPlayerTransform.rotation);
            //
            // NetworkServer.Spawn(unitSpawnerInstance, conn);
        }

        public override void OnServerSceneChanged(string sceneName)
        {
            if (SceneManager.GetActiveScene().name.StartsWith("Scene_Map"))
            {
                var gameOverHandlerInstance = Instantiate(gameOverHandlerPrefab);

                NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

                foreach (var player in Players)
                {
                    var baseInstance = Instantiate(
                        unitBasePrefab, 
                        GetStartPosition().position, 
                        Quaternion.identity);
                    
                    NetworkServer.Spawn(baseInstance, player.connectionToClient);
                }
            }
        }

        #endregion

        #region Client

        public override void OnClientConnect()
        {
            base.OnClientConnect();

            ClientOnConnected?.Invoke();
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();

            ClientOnDisconnected?.Invoke();
        }

        public override void OnStopClient()
        {
            Players.Clear();
        }

        #endregion
    }
}