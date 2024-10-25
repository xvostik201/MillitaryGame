using UnityEngine;

public class MilitaryVehicleController : MonoBehaviour
{
    [Header("Корпус")]
    [Tooltip("Объект, отвечающий за движение и повороты корпуса")]
    [SerializeField] private Transform _body;
    [Tooltip("Скорость движения корпуса")]
    [SerializeField] private float _bodySpeed = 3f;
    [Tooltip("Скорость поворота корпуса")]
    [SerializeField] private float _bodyRotationSpeed = 85f;
    private float _horizontalBodyAngle = 0f;

    [Header("Башня")]
    [Tooltip("Объект башни")]
    [SerializeField] private Transform _tower;
    [Tooltip("Позиция башни на танке")]
    [SerializeField] private Transform _towerOnTankPosition;
    [Tooltip("Скорость вращения башни")]
    [SerializeField] private float _towerRotationSpeed;

    [Tooltip("Список Rigidbody для отсоединения частей башни при разрушении")]
    [SerializeField] private Rigidbody[] _towerRigidbodies;

    [Header("Пушка")]
    [Tooltip("Объект пушки")]
    [SerializeField] private Transform _gun;
    [Tooltip("Минимальный угол наклона пушки")]
    [SerializeField] private float _minGunClamp = -25f;
    [Tooltip("Максимальный угол наклона пушки")]
    [SerializeField] private float _maxGunClamp = 17f;
    //[Tooltip("Скорость поворота пушки")]
    //[SerializeField] private float _gunRotationSpeed = 15f;

    [Tooltip("Точка, откуда будет происходить выстрел")]
    [SerializeField] private Transform _shootPoint;
    [Tooltip("Скорострельность")]
    [SerializeField] private float _shootRate = 0.1f;
    [Tooltip("Сила выстрела")]
    [SerializeField] private float _shootForce = 75f;
    private float _shootTimer;

    [Header("Вращение дула для зенитных орудий")]
    [Tooltip("Объект дула для вращения")]
    [SerializeField] private Transform _muzzle;
    [Tooltip("Скорость вращения дула, при которой можно стрелять")]
    [SerializeField] private float _muzzleRotationSpeedToShoot = 900f;
    [Tooltip("Скорость ускорения вращения дула")]
    [SerializeField] private float _muzzleAccelerationSpeed = 300f;
    private float _currentMuzzleSpeed = 0f;
    private bool _isSpinning;
    private bool _canShoot;

    [Header("Боеприпасы")]
    [Tooltip("Префаб пули")]
    [SerializeField] private GameObject _bulletPrefab;

    [Header("Камера")]
    [Tooltip("Камера для управления танком")]
    [SerializeField] private Camera _tankCamera;
    [Tooltip("Цель камеры — центр танка")]
    [SerializeField] private Transform _cameraTarget;
    [Tooltip("Расстояние от камеры до цели")]
    [SerializeField] private float _cameraDistance = 10f;
    [Tooltip("Чувствительность камеры по горизонтали")]
    [SerializeField] private float _horizontalSensitivity = 100f;
    [Tooltip("Чувствительность камеры по вертикали")]
    [SerializeField] private float _verticalSensitivity = 100f;
    [Tooltip("Минимальный угол наклона камеры")]
    [SerializeField] private float _minVerticalAngle = -30f;
    [Tooltip("Максимальный угол наклона камеры")]
    [SerializeField] private float _maxVerticalAngle = 60f;
    private float _horizontalAngle = 0f;
    private float _verticalAngle = 0f;

    [Header("Дополнительно")]
    [Tooltip("Эффект выстрела")]
    [SerializeField] private ParticleSystem _shootSFX;
    [Tooltip("Сила разрушения башни при уничтожении танка")]
    [SerializeField] private float _deathForce;

    [Header("Здоровье")]
    [Tooltip("Здоровье танка")]
    [SerializeField] private int _health;

    [Header("Тип техники")]
    [Tooltip("Тип транспортного средства: Танк или зенитка")]
    [SerializeField] private TypeOfVehicle _vehicleType;

    private enum TypeOfVehicle
    {
        Tank,  
        AA     
    }

    void Update()
    {
        ControlCamera();
        RotateTowerAndGun();
        HandleShooting();
        Movement();
    }

    private void ControlCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * _horizontalSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * _verticalSensitivity * Time.deltaTime;

        _horizontalAngle += mouseX;
        _verticalAngle -= mouseY;

        _verticalAngle = Mathf.Clamp(_verticalAngle, _minVerticalAngle, _maxVerticalAngle);

        Quaternion rotation = Quaternion.Euler(_verticalAngle, _horizontalAngle, 0);
        Vector3 offset = new Vector3(0, 0, -_cameraDistance);
        _tankCamera.transform.position = _cameraTarget.position + rotation * offset;
        _tankCamera.transform.LookAt(_cameraTarget);
    }

    private void RotateTowerAndGun()
    {
        _tower.position = _towerOnTankPosition.position;
        float cameraYRotation = _tankCamera.transform.eulerAngles.y;
        Quaternion targetYRotation = Quaternion.Euler(0, cameraYRotation, 0);
        _tower.rotation = Quaternion.Slerp(_tower.rotation, targetYRotation, _towerRotationSpeed * Time.deltaTime);

        float cameraXRotation = _tankCamera.transform.eulerAngles.x;
        cameraXRotation = (cameraXRotation > 180) ? cameraXRotation - 360 : cameraXRotation;
        cameraXRotation = Mathf.Clamp(cameraXRotation, _minGunClamp, _maxGunClamp);

        _gun.localRotation = Quaternion.Euler(cameraXRotation, 0, 0);
    }

    private void HandleShooting()
    {
        _shootTimer += Time.deltaTime;
        switch (_vehicleType)
        {
            case TypeOfVehicle.AA:
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    _isSpinning = true;
                    RotateMuzzle();

                    if (_canShoot && _shootTimer >= _shootRate)
                    {
                        Shoot();
                    }
                }
                else
                {
                    _isSpinning = false;
                    _canShoot = false;
                    _currentMuzzleSpeed = 0;
                }
                break;
            case TypeOfVehicle.Tank:
                if (Input.GetKey(KeyCode.Mouse0) && _shootTimer >= _shootRate)
                {
                    Shoot();
                }
                break;
        }
    }

    private void RotateMuzzle()
    {
        if (_isSpinning)
        {
            _currentMuzzleSpeed = Mathf.MoveTowards(_currentMuzzleSpeed, _muzzleRotationSpeedToShoot, _muzzleAccelerationSpeed * Time.deltaTime);
            _muzzle.Rotate(0, 0, _currentMuzzleSpeed * Time.deltaTime);

            if (_currentMuzzleSpeed >= _muzzleRotationSpeedToShoot)
            {
                _canShoot = true;
            }
        }
    }

    private void Shoot()
    {
        _shootSFX.Play();
        GameObject bulletObj = Instantiate(_bulletPrefab, _shootPoint.position, _shootPoint.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Shoot(_shootPoint.forward, _shootForce);
        _shootTimer = 0;
    }

    private void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal") * _bodyRotationSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * _bodySpeed * Time.deltaTime;

        _horizontalBodyAngle += horizontal;

        _body.position += transform.forward * vertical;
        _body.rotation = Quaternion.Euler(0, _horizontalBodyAngle, 0);
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;
        if (_health <= 0)
        {
            DestroyTank();
        }
    }

    private void DestroyTank()
    {
        foreach (Rigidbody bodies in _towerRigidbodies)
        {
            bodies.transform.parent = null;
            bodies.isKinematic = false;
            float randomAngle = Random.Range(-45, 45);
            bodies.AddForce(transform.up * _deathForce, ForceMode.Impulse);
            bodies.AddTorque(new Vector3(0, 0, randomAngle));
            enabled = false;
        }
    }
}
