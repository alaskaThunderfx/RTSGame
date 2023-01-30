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
    }
}