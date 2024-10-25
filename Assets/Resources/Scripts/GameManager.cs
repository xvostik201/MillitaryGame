using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Holders")]
    [SerializeField] private Camera[] _cameras; 
    //private int _currentCameraIndex = 0;

    [Header("States")]
    [SerializeField] private bool _isMainCameraActive = true;

    [Header("Game Objects")]
    [SerializeField] private DroneController _drone;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            DroneCameraToggle();
        }
    }

    private void DroneCameraToggle()
    {
        if (_isMainCameraActive)
        {

        }
        else
        {
        }
        _drone.ToggleDroneActivation();
        _isMainCameraActive = !_isMainCameraActive; 

    }
}
