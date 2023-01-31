using Buildings;
using Mirror;
using UnityEngine;

namespace Combat
{
    public class Targeter : NetworkBehaviour
    {
        private Targetable _target;

        public Targetable GetTarget()
        {
            return _target;
        }

        public override void OnStartServer()
        {
            GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
        }

        public override void OnStopServer()
        {
            GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
        }


        [Command]
        public void CmdSetTarget(GameObject targetGameObject)
        {
            if (!targetGameObject.TryGetComponent<Targetable>(out var target)) return;

            _target = target;
        }

        [Server]
        public void ClearTarget()
        {
            _target = null;
        }

        [Server]
        private void ServerHandleGameOver()
        {
            ClearTarget();
        }
    }
}