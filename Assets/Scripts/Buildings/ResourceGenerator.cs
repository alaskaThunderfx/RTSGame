using System.Collections;
using Combat;
using Mirror;
using Networking;
using UnityEngine;

namespace Buildings
{
    public class ResourceGenerator : NetworkBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private int resourcesPreInterval = 10;
        [SerializeField] private float interval = 2;

        private float _timer;
        private RTSPlayer _player;

        // private void Start()
        // {
        //     StartCoroutine(NetworkClientWaitForSeconds());
        // }

        public override void OnStartServer()
        {
            _timer = interval;
            _player = connectionToClient.identity.GetComponent<RTSPlayer>();

            health.ServerOnDie += ServerHandleDie;
            GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
            GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
        }

        [ServerCallback]
        private void Update()
        {
            if (_player == null)
            {
                _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            }

            _timer -= Time.deltaTime;

            if (_timer <= 0)
            {
                _player.SetResources(_player.GetResources() + resourcesPreInterval);

                _timer += interval;
            }
        }

        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        private void ServerHandleGameOver()
        {
            enabled = false;
        }

        private IEnumerator NetworkClientWaitForSeconds()
        {
            yield return new WaitForSeconds(.5f);
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }
    }
}