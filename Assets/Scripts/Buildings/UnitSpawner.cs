using System;
using Combat;
using Mirror;
using Networking;
using TMPro;
using Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Buildings
{
    public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
    {
        [SerializeField] private Health health;
        [SerializeField] private Unit unitPrefab = null;
        [SerializeField] private Transform unitSpawnPoint = null;
        [SerializeField] private TMP_Text remainingUnitsText;
        [SerializeField] private Image unitProgressImage;
        [SerializeField] private int maxUnitQueue = 5;
        [SerializeField] private float spawnMoveRange = 7;
        [SerializeField] private float unitSpawnDuration = 5;
        [SerializeField] private float progressImageVelocity;

        [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
        private int _queuedUnits;

        [SyncVar] private float _unitTimer;

        private RTSPlayer _player;


        private void Update()
        {
            if (isServer)
            {
                ProduceUnits();
            }

            if (isClient)
            {
                UpdateTimerDisplay();
            }
        }


        #region Server

        public override void OnStartServer()
        {
            health.ServerOnDie += ServerHandleDie;
        }

        public override void OnStopServer()
        {
            health.ServerOnDie -= ServerHandleDie;
        }

        [Server]
        private void ServerHandleDie()
        {
            NetworkServer.Destroy(gameObject);
        }

        [Command]
        private void CmdSpawnUnit()
        {
            if (_queuedUnits == maxUnitQueue) return;

            var player = connectionToClient.identity.GetComponent<RTSPlayer>();
            if (player.GetResources() < unitPrefab.GetResourceCost()) return;

            _queuedUnits++;
            player.SetResources(player.GetResources() - unitPrefab.GetResourceCost());
        }

        [Server]
        private void ProduceUnits()
        {
            if (_queuedUnits == 0) return;

            _unitTimer += Time.deltaTime;

            if (_unitTimer < unitSpawnDuration) return;

            var unitInstance = Instantiate(unitPrefab.gameObject, unitSpawnPoint.position, unitSpawnPoint.rotation);

            NetworkServer.Spawn(unitInstance, connectionToClient);

            var spawnOffset = Random.insideUnitSphere * spawnMoveRange;
            spawnOffset.y = unitSpawnPoint.position.y;

            var unitMovement = unitInstance.GetComponent<UnitMovement>();
            unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

            _queuedUnits--;
            _unitTimer = 0;
        }

        #endregion

        #region Client

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!isOwned) return;

            CmdSpawnUnit();
        }

        private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
        {
            remainingUnitsText.text = newUnits.ToString();
        }

        private void UpdateTimerDisplay()
        {
            var newProgress = _unitTimer / unitSpawnDuration;

            if (newProgress < unitProgressImage.fillAmount)
            {
                unitProgressImage.fillAmount = newProgress;
            }
            else
            {
                unitProgressImage.fillAmount = Mathf.SmoothDamp(
                    unitProgressImage.fillAmount,
                    newProgress,
                    ref progressImageVelocity,
                    .1f);
            }
        }

        #endregion
    }
}