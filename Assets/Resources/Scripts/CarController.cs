using UnityEngine;

public class CarController : MonoBehaviour
{
    enum States { Driver, Gunner }

    [SerializeField] private WheelCollider[] _wheelColliders;
    [SerializeField] private Transform[] _wheelMeshes;
    [SerializeField] private float _motorTorque = 1500f;
    [SerializeField] private float _maxSteerAngle = 30f;

    [SerializeField] private Transform _targetTurretPoint;
    [SerializeField] private float _cameraTurretOffset;
    [SerializeField] private float _cameraTurretSensivity;
    [SerializeField] private float _verticalTurretMinClamp;
    [SerializeField] private float _verticalTurretMaxClamp;

    private float _horizontalRotation;
    private float _verticalRotation;

    [SerializeField] private Transform _targetDriverPoint;
    [SerializeField] private float _cameraDriverOffset;
    [SerializeField] private float _cameraDriverSensivity;
    [SerializeField] private float _verticalDriverMinClamp;
    [SerializeField] private float _verticalDriverMaxClamp;

    private float _horizontalDriverRotation;
    private float _verticalDriverRotation;

    [SerializeField] private Transform _tower;
    [SerializeField] private Transform _gun;
    [SerializeField] private float _gunMinClamp;
    [SerializeField] private float _gunMaxClamp;
    [SerializeField] private Transform _shootPoint;
    [SerializeField] private GameObject _bullet;

    [SerializeField] private Camera _turretCamera;
    [SerializeField] private Camera _driverCamera;

    private States _state = States.Driver;

    private void Start()
    {
        OnStateChanged(_state);
    }

    private void Update()
    {
        switch (_state)
        {
            case States.Driver:
                CarMovement();
                CameraController(ref _horizontalDriverRotation, ref _verticalDriverRotation,
                    _cameraDriverSensivity, _verticalDriverMinClamp, _verticalDriverMaxClamp, _cameraDriverOffset, _driverCamera, _targetDriverPoint);
                break;

            case States.Gunner:
                CameraController(ref _horizontalRotation, ref _verticalRotation,
                    _cameraTurretSensivity, _verticalTurretMinClamp, _verticalTurretMaxClamp, _cameraTurretOffset, _turretCamera, _targetTurretPoint);
                TurretController();
                break;
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            switch (_state)
            {
                case States.Driver:
                    _state = States.Gunner;
                    OnStateChanged(States.Gunner);
                    break;
                case States.Gunner:
                    _state = States.Driver;
                    OnStateChanged(States.Driver);
                    break;
            }
        }
    }

    private void CarMovement()
    {
        float steer = Input.GetAxis("Horizontal");
        float motor = Input.GetAxis("Vertical");

        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            _wheelColliders[i].motorTorque = 0f;
        }

        for (int i = 0; i < _wheelColliders.Length; i++)
        {
            if (i >= 2)
                _wheelColliders[i].motorTorque = motor * _motorTorque;

            if (i < 2)
                _wheelColliders[i].steerAngle = steer * _maxSteerAngle;

            UpdateWheelPosition(_wheelColliders[i], _wheelMeshes[i]);
        }
    }

    private void UpdateWheelPosition(WheelCollider collider, Transform mesh)
    {
        Vector3 pos;
        Quaternion quat;
        collider.GetWorldPose(out pos, out quat);

        mesh.position = pos;
        mesh.rotation = quat;
    }

    private void CameraController(ref float rotationX, ref float rotationY, float sensivity, float minClamp, float maxClamp, float offset, Camera camera, Transform targetPoint)
    {
        float mouseX = Input.GetAxis("Mouse X") * sensivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensivity * Time.deltaTime;

        rotationX += mouseX;
        rotationY -= mouseY;

        rotationY = Mathf.Clamp(rotationY, minClamp, maxClamp);

        Quaternion rotation = Quaternion.Euler(rotationY, rotationX, 0);
        Vector3 newOffset = new Vector3(0, 0, -offset);
        camera.transform.position = targetPoint.position + rotation * newOffset;
        camera.transform.LookAt(targetPoint.position);
    }

    private void TurretController()
    {
        float cameraYRotation = _turretCamera.transform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, cameraYRotation, 0);
        _tower.rotation = targetRotation;

        float cameraXRotation = _turretCamera.transform.eulerAngles.x;
        cameraXRotation = (cameraXRotation > 180) ? cameraXRotation - 360 : cameraXRotation;
        cameraXRotation = Mathf.Clamp(cameraXRotation, _gunMinClamp, _gunMaxClamp);

        _gun.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);
    }

    private void OnStateChanged(States state)
    {
        switch (state)
        {
            case States.Driver:
                _turretCamera.gameObject.SetActive(false);
                _driverCamera.gameObject.SetActive(true);
                break;
            case States.Gunner:
                _turretCamera.gameObject.SetActive(true);
                _driverCamera.gameObject.SetActive(false);
                break;
        }
    }
}
