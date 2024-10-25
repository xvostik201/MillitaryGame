using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    [SerializeField] private Camera[] _cameras;

    public Camera GetCamera(int index)
    {
        if (index >= 0 && index < _cameras.Length)
        {
            return _cameras[index];
        }
        else
        {
            return null;
        }
    }

    public void DisableAllCameras()
    {
        foreach (Camera cam in _cameras)
        {
            cam.gameObject.SetActive(false);
        }
    }

    public void EnableCamera(int index)
    {
        DisableAllCameras();
        Camera cam = GetCamera(index);
        if (cam != null)
        {
            cam.gameObject.SetActive(true);
        }
    }
}
