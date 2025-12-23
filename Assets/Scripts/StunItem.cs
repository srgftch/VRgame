// StunItem.cs - прикрепите к объектам, которые могут оглушать
using UnityEngine;

public class StunItem : MonoBehaviour
{
    [Header("Stun Settings")]
    public float stunDuration = 3f;
    public float stunForce = 10f; // Сила удара
    public bool requireVelocity = true; // Требуется ли скорость для оглушения
    public float minVelocity = 2f; // Минимальная скорость для срабатывания

    [Header("Effects")]
    public ParticleSystem hitEffect;
    public AudioClip hitSound;

    private Rigidbody rb;
    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Проверяем, что это мышь
        RatNavigation rat = collision.gameObject.GetComponent<RatNavigation>();
        if (rat != null)
        {
            // Если требуется скорость - проверяем
            if (requireVelocity && rb != null)
            {
                if (rb.velocity.magnitude < minVelocity)
                    return;
            }

            // Оглушаем мышь
            rat.Stun(stunDuration);

            // Можно добавить силу удара
            Rigidbody ratRb = collision.gameObject.GetComponent<Rigidbody>();
            if (ratRb != null && stunForce > 0)
            {
                Vector3 direction = (collision.transform.position - transform.position).normalized;
                ratRb.AddForce(direction * stunForce, ForceMode.Impulse);
            }

            // Воспроизводим эффекты
            PlayHitEffects(collision.contacts[0].point);
        }
    }

    void PlayHitEffects(Vector3 position)
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, position, Quaternion.identity);
        }

        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, position);
        }
    }


}