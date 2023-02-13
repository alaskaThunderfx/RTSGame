using Mirror;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menus
{
    public class LobbyMenu : MonoBehaviour
    {
        [SerializeField] private GameObject lobbyUI;
        [SerializeField] private Button startGameButton;
        [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];

        private void Start()
        {
            RTSNetworkManager.ClientOnConnected += HandleClientConnected;
            RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateUpdated;
            RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
        }

        private void OnDestroy()
        {
            RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
            RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateUpdated;
            RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
        }

        private void HandleClientConnected()
        {
            lobbyUI.SetActive(true);
        }

        private void ClientHandleInfoUpdated()
        {
            var players = ((RTSNetworkManager)NetworkManager.singleton).Players;

            for (var i = 0; i < players.Count; i++)
            {
                playerNameTexts[i].text = players[i].GetDisplayName();
            }

            for (var i = players.Count; i < playerNameTexts.Length; i++)
            {
                playerNameTexts[i].text = "Waiting for player...";
            }

            startGameButton.interactable = players.Count >= 2;
        }

        private void AuthorityHandlePartyOwnerStateUpdated(bool state)
        {
            startGameButton.gameObject.SetActive(state);
        }

        public void StartGame()
        {
            NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
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