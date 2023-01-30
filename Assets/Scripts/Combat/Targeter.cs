using Mirror;
using UnityEngine;

namespace Combat
{
    public class Targeter : NetworkBehaviour
    {
        [SerializeField] private Targetable target;

        #region Server

        [Command]
        public void CmdSetTarget(GameObject targetGameObject)
        {
            if (!targetGameObject.TryGetComponent<Targetable>(out var target)) return;

            this.target = target;
        }

        [Server]
        public void ClearTarget()
        {
            target = null;
        }

        #endregion

        #region Client

        #endregion
    }
}