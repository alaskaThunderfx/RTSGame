using System.Collections;
using Mirror;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Buildings
{
    public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Building building;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private LayerMask floorMask;

        private Camera _mainCamera;
        private BoxCollider _buildingCollider;
        private RTSPlayer _player;
        private GameObject _buildingPreviewInstance;
        private Renderer _buildingRendererInstance;

        private void Start()
        {
            _mainCamera = Camera.main;

            iconImage.sprite = building.GetIcon();
            priceText.text = building.GetPrice().ToString();

            _buildingCollider = building.GetComponent<BoxCollider>();
            
            // StartCoroutine(NetworkClientWaitForSeconds());
        }

        private void Update()
        {
            if (_player == null)
            {
                // _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
                StartCoroutine(NetworkClientWaitForSeconds());
            }

            if (_buildingPreviewInstance == null) return;
        
            UpdateBuildingPreview();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;

            if (_player.GetResources() < building.GetPrice()) return;

            _buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
            _buildingRendererInstance = _buildingPreviewInstance.GetComponentInChildren<Renderer>();
        
            _buildingPreviewInstance.SetActive(false);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_buildingPreviewInstance == null) return;

            var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, floorMask))
            {
                _player.CmdTryPlaceBuilding(building.GetId(), hit.point);
            }
        
            Destroy(_buildingPreviewInstance);
        }

        public void UpdateBuildingPreview()
        {
            var ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, floorMask)) return;

            _buildingPreviewInstance.transform.position = hit.point;

            if (!_buildingPreviewInstance.activeSelf)
            {
                _buildingPreviewInstance.SetActive(true);
            }

            var color = _player.CanPlaceBuilding(_buildingCollider, hit.point) ? Color.green : Color.red;
            
            _buildingRendererInstance.material.SetColor("_BaseColor", color);
        }

        private IEnumerator NetworkClientWaitForSeconds()
        {
            yield return new WaitForSeconds(.5f);
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }
    }
}
