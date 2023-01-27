using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Units
{
    public class UnitMovement : NetworkBehaviour
    {
        [SerializeField] private NavMeshAgent agent;

        private Camera _mainCamera;

        #region Server

        [Command]
        private void CmdMove(Vector3 position)
        {
            if (!NavMesh.SamplePosition(position, out var hit, 1, NavMesh.AllAreas)) return;

            agent.SetDestination(hit.position);
        }

        #endregion

        #region Client

        public override void OnStartAuthority()
        {
            _mainCamera = Camera.main;
        }

        [ClientCallback]
        private void Update()
        {
            if (!isOwned) return;

            if (!Mouse.current.rightButton.wasPressedThisFrame) return;

            var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity)) return;
        
            CmdMove(hit.point);
        }

        #endregion
    }
}