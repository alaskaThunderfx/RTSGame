using System;
using System.Collections.Generic;
using Buildings;
using Mirror;
using UnityEngine;
using Unit = Units.Unit;

namespace Networking
{
    public class RTSPlayer : NetworkBehaviour
    {
        [SerializeField] private Building[] buildings = new Building[0];

        [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
        private int _resources = 500;

        public event Action<int> ClientOnResourcesUpdated;

        private List<Unit> _myUnits = new();
        private List<Building> _myBuildings = new();

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

        [Server]
        public void SetResources(int newResources)
        {
            _resources = newResources;
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

            var buildingInstance =
                Instantiate(buildingToPlace.gameObject, point, buildingToPlace.transform.rotation);

            NetworkServer.Spawn(buildingInstance, connectionToClient);
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


        public override void OnStopClient()
        {
            if (!isClientOnly || !isOwned) return;

            Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
            Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
            Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
            Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
        }

        private void ClientHandleResourcesUpdated(int oldResources, int newResources)
        {
            ClientOnResourcesUpdated?.Invoke(newResources);
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