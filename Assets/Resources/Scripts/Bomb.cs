using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float _radius;
    [HideInInspector] public int damage { private get; set; }

    private float _currentRadius;

    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(DestroyWave(_radius));
        MilitaryVehicleController militaryVehicleController = collision.gameObject.GetComponent<MilitaryVehicleController>();
        if (militaryVehicleController == null)
        {
            Transform parent = collision.transform.parent;
            militaryVehicleController = parent.GetComponentInChildren<MilitaryVehicleController>();
            if (militaryVehicleController != null)
            {
                militaryVehicleController.TakeDamage(damage);
            }
        }
        else
        {
            militaryVehicleController.TakeDamage(damage);
        }
    }

    IEnumerator DestroyWave(float radius)
    {
        GetComponent<MeshRenderer>().enabled = false;
        while (_currentRadius < radius)
        {
            _currentRadius = Mathf.MoveTowards(_currentRadius, radius, 10 * Time.deltaTime);

            GetComponent<CapsuleCollider>().radius = _currentRadius;

            yield return null;
        }

        Destroy(gameObject);
    }
}
