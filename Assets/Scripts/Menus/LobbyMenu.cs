using System;
using Mirror;
using Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menus
{
    public class LobbyMenu : MonoBehaviour
    {
        [SerializeField] private GameObject lobbyUI;

        private void Start()
        {
            RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        }

        private void OnDestroy()
        {
            RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        }

        private void HandleClientConnected()
        {
            lobbyUI.SetActive(true);
        }

        public void LeaveLobby()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else
            {
                NetworkManager.singleton.StopClient();

                SceneManager.LoadScene(0);
            }
        }
    }
}