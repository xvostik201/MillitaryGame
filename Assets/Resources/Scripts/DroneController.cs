using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DroneController : MonoBehaviour
{
    [SerializeField] private Transform[] _wheels;
    [SerializeField] private Vector3[] _closeAngles;
    private bool _isWheelClose = false;
    private bool[] _isWheelWorking;
    private Quaternion[] _targetRotations;

    [SerializeField] private Transform _propeller;
    [SerializeField] private float _propellerSpeed;

    [SerializeField] private Transform _horizontalSensor;
    [SerializeField] private Transform _verticalSensor;
    [SerializeField] private float _sensorSensivity;
    private float _horizontalRotation = 0.0f;
    private float _verticalRotation = 0.0f;

    [SerializeField] private float _mouseScrollWheelSensivity;
    [SerializeField] private float _minFOVValue;
    [SerializeField] private float _maxFOXValue;
    [SerializeField] private Camera _droneCamera;

    [SerializeField] private PostProcessLayer _postProcessLayer;
    private bool _isThermalVisionActive;

    [SerializeField] private GameObject _airstrikePrefab;
    [SerializeField] private GameObject _hitPointPrefab;
    [SerializeField] private float _airstrikeRate;
    [SerializeField] private int _airstrikeDamage;
    private GameObject _lastPoint;
    private float _airstrikeTimer;
    private bool _isAirstrikeReady;

    [SerializeField] private GameObject _droneUI;
    [SerializeField] private TMP_Text _airstrikeReadyText;

    private bool _isDroneActive = false;
    private void Awake()
    {
        _airstrikeTimer = _airstrikeRate;
        _isDroneActive = true;
        _isWheelWorking = new bool[_wheels.Length];
        _targetRotations = new Quaternion[_wheels.Length];
    }

    void Update()
    {
        _airstrikeTimer += Time.deltaTime;
        Propeller();
        ChassisMechanization();

        if (!_isDroneActive) return;
        PrepareAirstrike();
        CameraSensor();
        Camera();

        DroneUI();
    }

    private void DroneUI()
    {
        _airstrikeReadyText.text = _isAirstrikeReady
                    ? _airstrikeReadyText.text = "Airstrike ready"
                    : _airstrikeReadyText.text = "Airstrike not ready";
        _droneUI.SetActive(_isDroneActive);
    }

    private void Camera()
    {
        float mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");
        _droneCamera.fieldOfView = Mathf.Clamp(_droneCamera.fieldOfView, _minFOVValue, _maxFOXValue);
        _droneCamera.fieldOfView -= mouseScrollWheel * Time.deltaTime * _mouseScrollWheelSensivity;

        if (_droneCamera.fieldOfView > _maxFOXValue)
        {
            _droneCamera.fieldOfView = _maxFOXValue;
        }
        else if (_droneCamera.fieldOfView < _minFOVValue)
        {
            _droneCamera.fieldOfView = _minFOVValue;
        }

        ThermalVision();
    }

    private void ThermalVision()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            _postProcessLayer.enabled = !_isThermalVisionActive;
            _isThermalVisionActive = !_isThermalVisionActive;
        }
    }

    private void CameraSensor()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensorSensivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _sensorSensivity * Time.deltaTime;

        _horizontalRotation += mouseX;
        _verticalRotation -= mouseY;

        _horizontalSensor.rotation = Quaternion.Euler(0, _horizontalRotation, 0);
        _verticalSensor.localRotation = Quaternion.Euler(0, 0, -_verticalRotation);
    }

    private void Propeller()
    {
        _propeller.Rotate(_propellerSpeed * Time.deltaTime, 0, 0);
    }

    private void ChassisMechanization()
    {
        if (Input.GetKeyDown(KeyCode.G) && _isDroneActive)
        {
            ToggleRotation();
        }

        for (int i = 0; i < _wheels.Length; i++)
        {
            if (_isWheelWorking[i])
            {
                RotateWheel(i);
            }
        }
    }

    private void ToggleRotation()
    {
        for (int i = 0; i < _wheels.Length; i++)
        {
            _targetRotations[i] = _isWheelClose
                ? Quaternion.Euler(0, 0, 0)
                : Quaternion.Euler(_closeAngles[i]);

            _isWheelWorking[i] = true;
        }

        _isWheelClose = !_isWheelClose;
    }

    private void RotateWheel(int index)
    {
        _wheels[index].rotation = Quaternion.Slerp(_wheels[index].rotation, _targetRotations[index], Time.deltaTime * 2f);

        if (Quaternion.Angle(_wheels[index].rotation, _targetRotations[index]) < 0.1f)
        {
            _wheels[index].rotation = _targetRotations[index];
            _isWheelWorking[index] = false;
        }
    }

    private void PrepareAirstrike()
    {
        _isAirstrikeReady = _airstrikeTimer >= _airstrikeRate;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(_droneCamera.transform.position, _droneCamera.transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 50f))
            {
                Debug.Log("Объект: " + hit.collider.name);
                Debug.Log("Точка столкновения: " + hit.point);

                if (_lastPoint != null)
                {
                    Destroy(_lastPoint);
                }

                _lastPoint = Instantiate(_hitPointPrefab, hit.point, hit.transform.rotation);

                if (_isAirstrikeReady)
                {
                    Airstrike();
                }
            }
        }
    }


    private void Airstrike()
    {
        _isAirstrikeReady = false;
        _airstrikeTimer = 0;
        Vector3 position = new Vector3(_lastPoint.transform.position.x, _lastPoint.transform.position.y + 20f, _lastPoint.transform.position.z);
        GameObject airstrike = Instantiate(_airstrikePrefab, position, Quaternion.identity);
        airstrike.GetComponent<Bomb>().damage = _airstrikeDamage;
    }

    public void ToggleDroneActivation()
    {
        _isDroneActive = !_isDroneActive;
    }
}
