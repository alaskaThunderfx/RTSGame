using System;
using System.Collections.Generic;
using Buildings;
using Mirror;
using UnityEngine;
using Units;

namespace Networking
{
    public class RTSPlayer : NetworkBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LayerMask buildingBlockLayer;
        [SerializeField] private Building[] buildings = new Building[0];
        [SerializeField] private float buildingRangeLimit = 5;

        [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
        private int _resources = 500;

        [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
        private bool _isPartyOwner = false;

        public event Action<int> ClientOnResourcesUpdated;

        public static event Action<bool> AuthorityOnPartyOwnerStateUpdate;

        private Color _teamColor;
        private List<Unit> _myUnits = new();
        private List<Building> _myBuildings = new();

        public bool GetIsPartyOwner()
        {
            return _isPartyOwner;
        }
        
        public Transform GetCameraTransform()
        {
            return cameraTransform;
        }

        public Color GetTeamColor()
        {
            return _teamColor;
        }

        public int GetResources()
        {
            return _resources;
        }

        public List<Unit> GetMyUnits()
        {
            return _myUnits;
        }

        public List<Building> GetMyBuildings()
        {
            return _myBuildings;
        }

        public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
        {
            if (Physics.CheckBox(
                    point + buildingCollider.center,
                    buildingCollider.size / 2,
                    Quaternion.identity,
                    buildingBlockLayer))
            {
                return false;
            }

            foreach (var building in _myBuildings)
            {
                if ((point - building.transform.position).sqrMagnitude
                    <= buildingRangeLimit * buildingRangeLimit)
                {
                    return true;
                }
            }

            return false;
        }

        #region Server

        public override void OnStartServer()
        {
            Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
            Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
            Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
            Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;
        }

        public override void OnStopServer()
        {
            Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
            Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
            Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
            Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
        }

        [Server]
        public void SetPartyOwner(bool state)
        {
            _isPartyOwner = state;
        }

        [Server]
        public void SetTeamColor(Color newTeamColor)
        {
            _teamColor = newTeamColor;
        }

        [Server]
        public void SetResources(int newResources)
        {
            _resources = newResources;
        }

        [Command]
        public void CmdStartGame()
        {
            if (!_isPartyOwner) return;
            
            ((RTSNetworkManager)NetworkManager.singleton).StartGame();
        }

        [Command]
        public void CmdTryPlaceBuilding(int buildingId, Vector3 point)
        {
            Building buildingToPlace = null;

            foreach (var building in buildings)
            {
                if (building.GetId() == buildingId)
                {
                    buildingToPlace = building;
                    break;
                }
            }

            if (buildingToPlace == null) return;

            if (_resources < buildingToPlace.GetPrice()) return;

            var buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

            if (!CanPlaceBuilding(buildingCollider, point)) return;

            var buildingInstance =
                Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);

            NetworkServer.Spawn(buildingInstance, connectionToClient);

            SetResources(_resources - buildingToPlace.GetPrice());
        }

        private void ServerHandleBuildingSpawned(Building building)
        {
            if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;

            _myBuildings.Add(building);
        }

        private void ServerHandleBuildingDespawned(Building building)
        {
            if (building.connectionToClient.connectionId != connectionToClient.connectionId) return;

            _myBuildings.Remove(building);
        }

        private void ServerHandleUnitSpawned(Unit unit)
        {
            if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

            _myUnits.Add(unit);
        }

        private void ServerHandleUnitDespawned(Unit unit)
        {
            if (unit.connectionToClient.connectionId != connectionToClient.connectionId) return;

            _myUnits.Remove(unit);
        }

        #endregion

        #region Client

        public override void OnStartAuthority()
        {
            if (NetworkServer.active) return;

            Unit.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
            Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
            Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
            Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
        }


        public override void OnStartClient()
        {
            if (NetworkServer.active) return;
            
            ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
        }

        public override void OnStopClient()
        {
            if (!isClientOnly) return;
            
            ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

            if (!isOwned) return;

            Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
            Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
            Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
            Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
        }

        private void ClientHandleResourcesUpdated(int oldResources, int newResources)
        {
            ClientOnResourcesUpdated?.Invoke(newResources);
        }

        private void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
        {
            if (!isOwned) return;

            AuthorityOnPartyOwnerStateUpdate?.Invoke(newState);
        }

        private void AuthorityHandleUnitSpawned(Unit unit)
        {
            _myUnits.Add(unit);
        }

        private void AuthorityHandleUnitDespawned(Unit unit)
        {
            _myUnits.Remove(unit);
        }

        private void AuthorityHandleBuildingSpawned(Building building)
        {
            _myBuildings.Add(building);
        }

        private void AuthorityHandleBuildingDespawned(Building building)
        {
            _myBuildings.Remove(building);
        }

        #endregion
    }
}