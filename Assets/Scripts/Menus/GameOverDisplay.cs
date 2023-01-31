using Buildings;
using Mirror;
using TMPro;
using UnityEngine;

namespace Menus
{
    public class GameOverDisplay : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverDisplayParent;
        [SerializeField] private TMP_Text winnerNameText;

        private void Start()
        {
            GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
        }

        private void OnDestroy()
        {
            GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
        }

        public void LeaveGame()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
            {
                NetworkManager.singleton.StopHost();
            }
            else
            {
                NetworkManager.singleton.StopClient();
            }
        }

        private void ClientHandleGameOver(string winner)
        {
            winnerNameText.text = $"{winner} Has Won!";
            
            gameOverDisplayParent.SetActive(true);
        }
    }
}
