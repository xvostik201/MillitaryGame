using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] private float _playerMoveSpeed = 3f;
    private CharacterController _charaController;

    [Header("Camera Settings")]
    [SerializeField] private float _sensivity = 130f;
    [SerializeField] private Camera _camera;
    private float _minClamp = -30f;
    private float _maxClamp = 60f;
    private float _horizontalRotation;
    private float _verticalRotation;

    private Animator _animator;

    private void Awake()
    {
        _charaController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        Movement();
        CameraController();
    }

    private void CameraController()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _sensivity * Time.deltaTime;

        _horizontalRotation += mouseX;
        _verticalRotation -= mouseY;

        _verticalRotation = Mathf.Clamp(_verticalRotation, _minClamp, _maxClamp);

        _camera.transform.rotation = Quaternion.Euler(_verticalRotation, _horizontalRotation, 0);

        Quaternion rotation = Quaternion.Euler(0, _horizontalRotation, 0);
        transform.rotation = rotation;
    }

    private void Movement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = transform.right * horizontal + transform.forward * vertical;

        Vector3 normalizedDirection = direction.normalized * _playerMoveSpeed * Time.deltaTime;

        _charaController.Move(normalizedDirection);

        _animator.SetFloat("Horizontal", horizontal, 0.2f, Time.deltaTime);
        _animator.SetFloat("Vertical", vertical, 0.2f, Time.deltaTime);
    }

    
}
