using System;
using UnityEngine;

namespace Cameras
{
    public class FaceCamera : MonoBehaviour
    {
        private Transform _mainCameraTransform;

        private void Start()
        {
            if (Camera.main != null) _mainCameraTransform = Camera.main.transform;
        }

        private void LateUpdate()
        {
            transform.LookAt(
                transform.position + _mainCameraTransform.rotation * Vector3.forward,
                _mainCameraTransform.rotation * Vector3.up
            );
        }
    }
}