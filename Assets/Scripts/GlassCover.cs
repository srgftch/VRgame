using UnityEngine;

public class GlassCover : MonoBehaviour
{
    [Header("Glass Break Settings")]
    public float breakForceThreshold = 5f; // Минимальная сила для разрушения
    public GameObject brokenGlassPrefab; // Префаб с разбитым стеклом
    public AudioClip breakSound; // Звук разбития
    public ParticleSystem breakParticles; // Эффект разбития

    private Rigidbody rb;
    private bool isBroken = false;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Изначально крышка статична
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

        // Проверяем, что столкнулись с ломом
        Crowbar crowbar = collision.gameObject.GetComponent<Crowbar>();
        if (crowbar != null)
        {
            // Рассчитываем силу удара
            float impactForce = collision.impulse.magnitude / Time.fixedDeltaTime;
            Debug.Log($"Удар ломом! Сила: {impactForce}, Порог: {breakForceThreshold}");

            if (impactForce >= breakForceThreshold)
            {
                BreakGlass(collision.contacts[0].point);
            }
        }
    }

    public void BreakGlass(Vector3 breakPoint)
    {
        if (isBroken) return;

        isBroken = true;
        Debug.Log("Стекло разбито!");

        // Проигрываем звук
        if (breakSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(breakSound);
        }

        // Включаем эффекты частиц
        if (breakParticles != null)
        {
            breakParticles.transform.position = breakPoint;
            breakParticles.Play();
        }

        // Создаем осколки
        if (brokenGlassPrefab != null)
        {
            GameObject brokenGlass = Instantiate(brokenGlassPrefab, transform.position, transform.rotation);

            // Передаем импульс осколкам (опционально)
            Rigidbody[] fragments = brokenGlass.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody fragment in fragments)
            {
                fragment.AddExplosionForce(2f, breakPoint, 3f);
            }
        }

        // Отключаем или скрываем целое стекло
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Уничтожаем объект через 5 секунд (после того как осколки упадут)
        Destroy(gameObject, 5f);
    }

    // Метод для принудительного разбития (например, для тестирования)
    [ContextMenu("Разбить стекло")]
    public void ForceBreak()
    {
        BreakGlass(transform.position);
    }
}