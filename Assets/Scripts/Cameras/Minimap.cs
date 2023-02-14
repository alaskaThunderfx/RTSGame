using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Networking;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Cameras
{
    public class Minimap : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private RectTransform minimapRect;
        [SerializeField] private float mapScale = 20;
        [SerializeField] private float offset = -6;

        private Transform _playerCameraTransform;

        private void Start()
        {
            StartCoroutine(NetworkClientWaitForSeconds());
        }

        private void Update()
        {
            if (_playerCameraTransform != null) return;

            if (NetworkClient.connection?.identity == null) return;
            
            _playerCameraTransform = NetworkClient.connection.identity
                .GetComponent<RTSPlayer>().GetCameraTransform();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            MoveCamera();
        }

        public void OnDrag(PointerEventData eventData)
        {
            MoveCamera();
        }

        private void MoveCamera()
        {
            var mousePos = Mouse.current.position.ReadValue();

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    minimapRect,
                    mousePos,
                    null,
                    out var localPoint
                )) return;

            var lerp = new Vector2(
                (localPoint.x - minimapRect.rect.x) / minimapRect.rect.width,
                (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height
            );

            var newCameraPos = new Vector3(
                Mathf.Lerp(-mapScale, mapScale, lerp.x),
                _playerCameraTransform.position.y,
                Mathf.Lerp(-mapScale, mapScale, lerp.y)
            );

            _playerCameraTransform.position = newCameraPos + new Vector3(0, 0, offset);
        }

        private IEnumerator NetworkClientWaitForSeconds()
        {
            yield return new WaitForSeconds(.5f);
            _playerCameraTransform = NetworkClient.connection.identity.GetComponent<RTSPlayer>().GetCameraTransform();
        }
    }
}