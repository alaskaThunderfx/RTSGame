using System;
using Mirror;
using UnityEngine;

namespace Networking
{
    public class TeamColorSetter : NetworkBehaviour
    {
        [SerializeField] private Renderer[] colorRenderers = Array.Empty<Renderer>();

        [SyncVar(hook = nameof(HandleTeamColorUpdated))]
        private Color _teamColor = new Color();

        #region Server

        public override void OnStartServer()
        {
            Debug.Log(gameObject.name);
            var player = connectionToClient.identity.GetComponent<RTSPlayer>();

            _teamColor = player.GetTeamColor();
        }

        #endregion

        #region Client

        private void HandleTeamColorUpdated(Color oldColor, Color newColor)
        {
            foreach (var renderer in colorRenderers)
            {
                renderer.material.SetColor("_BaseColor", newColor);
                renderer.material.SetColor("_EmissionColor", newColor);
            }
        }

        #endregion
    }
}