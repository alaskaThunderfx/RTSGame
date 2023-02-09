using Mirror;
using UnityEngine;

namespace Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject landingPagePanel;

        public void HostLobby()
        {
            landingPagePanel.SetActive(false);
        
            NetworkManager.singleton.StartHost();
        }
    }
}
