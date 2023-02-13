using Combat;
using Mirror;
using Unity.Mathematics;
using UnityEngine;

namespace Units
{
    public class UnitFiring : NetworkBehaviour
    {
        [SerializeField] private Targeter targeter;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform projectileSpawnPoint;
        [SerializeField] private float fireRange = 5;
        [SerializeField] private float fireRate = 1;
        [SerializeField] private float rotationSpeed = 20;

        private float _lastFireTime;

        [Server]
        private void Update()
        {
            var target = targeter.GetTarget();
            if (target == null) return;

            if (!CanFireAtTarget()) return;

            var targetRotation =
                Quaternion.LookRotation(target.transform.position - transform.position);

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime);

            if (Time.time > 1 / fireRate + _lastFireTime)
            {
                var projectileRotation =
                    Quaternion.LookRotation(
                        target.GetAimAtPoint().position
                        - projectileSpawnPoint.position);

                var projectileInstance =
                    Instantiate(
                        projectilePrefab,
                        projectileSpawnPoint.position,
                        projectileRotation);

                NetworkServer.Spawn(projectileInstance, connectionToClient);

                _lastFireTime = Time.time;
            }
        }

        [Server]
        private bool CanFireAtTarget()
        {
            return (targeter.GetTarget().transform.position - transform.position).sqrMagnitude
                   <= fireRange * fireRange;
        }
    }
}