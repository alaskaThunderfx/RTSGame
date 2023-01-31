using System;
using Combat;
using Mirror;
using UnityEngine;

namespace Buildings
{
    public class UnitBase : NetworkBehaviour
    {
        [SerializeField] private Health health;

        public static event Action<UnitBase> ServerOnBaseSpawned; 
        public static event Action<UnitBase> ServerOnBaseDespawned; 

        #region Server

        public override void OnStartServer()
        {
            health.ServerOnDie += ServerHandleDie;
            
            ServerOnBaseSpawned?.Invoke(this);
        }

        public override void OnStopServer()
        {
            ServerOnBaseDespawned?.Invoke(this);
            
            health.ServerOnDie -= ServerHandleDie;
        }

        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        #endregion

        #region Client

        #endregion
    }
}