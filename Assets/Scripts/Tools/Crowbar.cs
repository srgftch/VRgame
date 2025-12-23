using UnityEngine;

public class Crowbar : MonoBehaviour
{
    [Header("Crowbar Settings")]
    public float impactForceMultiplier = 1.5f; // Множитель силы удара
    public float minImpactVelocity = 2f; // Минимальная скорость для регистрации удара
    public AudioClip[] hitSounds; // Звуки ударов
    public ParticleSystem hitParticles; // Эффект при ударе

    private Rigidbody rb;
    private AudioSource audioSource;
    private Vector3 previousVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.sleepThreshold = 0.1f; // Меньше порог сна для лучшей реакции
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f; // Полностью 3D звук
            audioSource.minDistance = 0.5f;
            audioSource.maxDistance = 10f;
        }
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            previousVelocity = rb.velocity;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Рассчитываем относительную скорость
        float impactVelocity = previousVelocity.magnitude;

        if (impactVelocity < minImpactVelocity) return;

        // Проверяем, что ударили в стекло
        GlassCover glass = collision.gameObject.GetComponent<GlassCover>();
        if (glass != null)
        {
            // Передаем информацию об ударе в стекло
            glass.HandleCrowbarImpact(collision, impactVelocity * impactForceMultiplier);

            // Визуальные и звуковые эффекты для лома
            PlayHitEffects(collision.contacts[0].point);
        }
    }

    private void PlayHitEffects(Vector3 hitPoint)
    {
        // Звук удара
        if (hitSounds != null && hitSounds.Length > 0 && audioSource != null)
        {
            AudioClip randomHitSound = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomHitSound);
        }

        // Эффект частиц
        if (hitParticles != null)
        {
            hitParticles.transform.position = hitPoint;
            hitParticles.Play();
        }
    }
}