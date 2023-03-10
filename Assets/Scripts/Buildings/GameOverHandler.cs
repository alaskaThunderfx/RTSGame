using System;
using System.Collections.Generic;
using Mirror;
using Networking;
using UnityEngine;

namespace Buildings
{
    public class GameOverHandler : NetworkBehaviour
    {
        public static event Action ServerOnGameOver;
        public static event Action<string> ClientOnGameOver;

        private List<UnitBase> _bases = new();

        #region Server

        public override void OnStartServer()
        {
            UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
        }

        public override void OnStopServer()
        {
            UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
            UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
        }

        [Server]
        private void ServerHandleBaseSpawned(UnitBase unitBase)
        {
            _bases.Add(unitBase);
        }

        [Server]
        private void ServerHandleBaseDespawned(UnitBase unitBase)
        {
            _bases.Remove(unitBase);

            if (_bases.Count != 1) return;

            var playerId = _bases[0].connectionToClient.connectionId;

            RpcGameOver($"Player {playerId}");

            ServerOnGameOver?.Invoke();
        }

        #endregion

        #region Client

        [ClientRpc]
        private void RpcGameOver(string winner)
        {
            ClientOnGameOver?.Invoke(winner);
        }

        #endregion
    }
}