using System.Collections;
using System.Collections.Generic;
using Mirror;
using Networking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Units
{
    public class UnitSelectionHandler : MonoBehaviour
    {
        [SerializeField] private RectTransform unitSelectionArea = null;
        [SerializeField] private LayerMask layerMask = new LayerMask();

        private Vector2 _startPosition;
        private RTSPlayer _player;
        private Camera _mainCamera;

        public List<Unit> SelectedUnits { get; } = new();

        private void Start()
        {
            _mainCamera = Camera.main;
            StartCoroutine(NetworkClientWaitForSeconds());
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartSelectionArea();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ClearSelectionArea();
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                UpdateSelectionArea();
            }
        }

        private IEnumerator NetworkClientWaitForSeconds()
        {
            yield return new WaitForSeconds(.5f);
            _player = _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }

        private void StartSelectionArea()
        {
            if (!Keyboard.current.leftShiftKey.isPressed)
            {
                foreach (var selectedUnit in SelectedUnits)
                {
                    selectedUnit.Deselect();
                }

                SelectedUnits.Clear();
            }


            unitSelectionArea.gameObject.SetActive(true);

            _startPosition = Mouse.current.position.ReadValue();

            UpdateSelectionArea();
        }

        private void UpdateSelectionArea()
        {
            var mousePosition = Mouse.current.position.ReadValue();

            var areaWidth = mousePosition.x - _startPosition.x;
            var areaHeight = mousePosition.y - _startPosition.y;

            unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
            unitSelectionArea.anchoredPosition = _startPosition + new Vector2(areaWidth / 2, areaHeight / 2);
        }

        private void ClearSelectionArea()
        {
            unitSelectionArea.gameObject.SetActive(false);

            if (unitSelectionArea.sizeDelta.magnitude == 0)
            {
                var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

                if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, layerMask)) return;
                if (!hit.collider.TryGetComponent<Unit>(out var unit)) return;
                if (!unit.isOwned) return;

                SelectedUnits.Add(unit);

                foreach (var selectedUnit in SelectedUnits)
                {
                    selectedUnit.Select();
                }

                return;
            }

            var min = unitSelectionArea.anchoredPosition - unitSelectionArea.sizeDelta / 2;
            var max = unitSelectionArea.anchoredPosition + unitSelectionArea.sizeDelta / 2;

            foreach (var unit in _player.GetMyUnits())
            {
                if (SelectedUnits.Contains(unit)) continue;
                
                var screenPosition = _mainCamera.WorldToScreenPoint(unit.transform.position);

                if (screenPosition.x > min.x &&
                    screenPosition.x < max.x &&
                    screenPosition.y > min.y &&
                    screenPosition.y < max.y)
                {
                    SelectedUnits.Add(unit);
                    unit.Select();
                }
            }
        }
    }
}