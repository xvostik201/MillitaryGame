using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody _rb;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 direction, float force)
    {
        _rb.AddForce(direction * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        MilitaryVehicleController militaryVehicleController = collision.gameObject.GetComponent<MilitaryVehicleController>();
        if (militaryVehicleController == null)
        {
            Transform parent = collision.transform.parent;
            militaryVehicleController = parent.GetComponentInChildren<MilitaryVehicleController>();
            if (militaryVehicleController != null)
            {
                militaryVehicleController.TakeDamage(1);
            }
        }
        else
        {
            militaryVehicleController.TakeDamage(1);
        }
        Destroy(gameObject);
    }
}
