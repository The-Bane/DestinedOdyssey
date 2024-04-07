using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using System;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime;

    private float _distanceToPlayer;
    private Vector3 _currentVelocity = Vector3.zero;

    private Vector2 _delta;

    public float zoomSpeed = 4f;
    public float minZoom = 5f;
    public float maxZoom = 15f;
    private float currentZoom = 7f;
    /*
    [SerializeField] private MouseSensitivity mouseSensitivity;
    [SerializeField] private CameraAngle cameraAngle;
    private CameraRotation _cameraRotation;
*/
    private bool _isBusy;
    private bool _isRotating;
    private float _xRotation;

//    [SerializeField] private float movementSpeed = 10.0f;
    [SerializeField] private float rotationSpeed = 0.5f;

    private void Awake()
    {
        _xRotation = transform.rotation.eulerAngles.x;
        _distanceToPlayer = Vector3.Distance(transform.position, target.position);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _delta = context.ReadValue<Vector2>();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        if (_isBusy)
        {
            return;
        }

        _isRotating = context.started || context.performed;

        if (context.canceled)
        {
            _isBusy = true;
            SnapRotation();
        }
    }

    private void Update()
    {/*
        _cameraRotation.Yaw += _delta.x * mouseSensitivity.horizontal * Time.deltaTime;
        _cameraRotation.Pitch += _delta.y * mouseSensitivity.vertical * Time.deltaTime;
        _cameraRotation.Pitch = Mathf.Clamp(_cameraRotation.Pitch, cameraAngle.min, cameraAngle.max);
        */

        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
    }

    private void LateUpdate()
    {
        if (_isRotating)
        {
            transform.Rotate(new Vector3(_xRotation, -_delta.x * rotationSpeed, 0.0f));
            transform.rotation = Quaternion.Euler(_xRotation, transform.rotation.eulerAngles.y, 0.0f);
        }

        var targetPosition = target.position - transform.forward * _distanceToPlayer;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
        /*
        transform.eulerAngles = new Vector3(_cameraRotation.Pitch, _cameraRotation.Yaw, 0.0f);
        transform.position = target.position - transform.forward * _distanceToPlayer;
        */
    }

    private void SnapRotation()
    {
        transform.DORotate(SnappedVector(), 0.5f)
            .SetEase(Ease.OutBounce)
            .OnComplete(() =>
            {
                _isBusy = false;
            });
    }

    private Vector3 SnappedVector()
    {
        float endValue = 0.0f;
        float currentY = Mathf.Ceil(transform.rotation.eulerAngles.y);

        endValue = currentY switch
        {
            >= 0 and <= 90 => 45.0f,
            >=91 and <= 180 => 135.0f,
            >=181 and <= 270 => 225.0f,
            _ => 315.0f
        };

        return new Vector3(_xRotation, endValue, 0.0f);
    }

    [Serializable]
    public struct MouseSensitivity
    {
        public float horizontal;
        public float vertical;
    }

    public struct CameraRotation
    {
        public float Pitch;
        public float Yaw;
    }

    [Serializable]
    public struct CameraAngle
    {
        public float min;
        public float max;
    }
}
