using UnityEngine;

public class MouseCorpse : MonoBehaviour
{
    [Header("Settings")]
    public bool canBeGrabbed = true;
    public float grabHeight = 0.3f;

    [Header("Effects")]
    public ParticleSystem destroyEffect;
    public AudioClip destroySound;
    public AudioClip grabSound;

    private Rigidbody rb;
    private bool isGrabbed = false;
    private Transform grabPoint;
    private AudioSource audioSource;
    private bool alreadyDestroyed = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
    }

    public void Grab(Transform grabberHand)
    {
        if (!canBeGrabbed || isGrabbed) return;

        isGrabbed = true;
        grabPoint = grabberHand;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (grabSound != null && audioSource != null)
            audioSource.PlayOneShot(grabSound);
    }

    public void Release()
    {
        isGrabbed = false;
        grabPoint = null;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    void LateUpdate()
    {
        if (isGrabbed && grabPoint != null)
        {
            transform.position = grabPoint.position + Vector3.up * grabHeight;
            transform.rotation = grabPoint.rotation;
        }
    }

    public void DestroyCorpse()
    {
        if (destroyEffect != null)
            Instantiate(destroyEffect, transform.position, Quaternion.identity);

        if (destroySound != null)
            AudioSource.PlayClipAtPoint(destroySound, transform.position);

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MouseTrap") && !alreadyDestroyed)
        {
            alreadyDestroyed = true;
            DestroyCorpse();
        }
    }
}