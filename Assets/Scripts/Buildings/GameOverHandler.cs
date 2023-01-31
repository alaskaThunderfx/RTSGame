using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Buildings
{
    public class GameOverHandler : NetworkBehaviour
    {
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
            
            Debug.Log("Game Over!");
        }

        #endregion

        #region Client

        

        #endregion
    }
}
