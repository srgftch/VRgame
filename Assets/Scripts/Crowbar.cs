using UnityEngine;

public class Crowbar : MonoBehaviour
{
    [Header("Crowbar Settings")]
    public float baseForceMultiplier = 1f; // Множитель силы удара

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Настраиваем физику лома для лучших ощущений удара
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
}