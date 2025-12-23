using UnityEngine;

public class MouseTrapZone : MonoBehaviour
{
    [Header("Settings")]
    public string mouseTag = "MouseCorpse";
    public int scorePerMouse = 1;

    [Header("Effects")]
    public ParticleSystem captureEffect;
    public AudioClip captureSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Проверяем, что это мышь
        if (!IsMouse(other)) return;

        // Получаем главный объект мыши (через родителя или root)
        GameObject mouseObject = GetMouseRootObject(other.gameObject);

        if (mouseObject != null)
        {
            ProcessMouseCapture(mouseObject);
        }
    }

    bool IsMouse(Collider collider)
    {
        // Проверяем тег или компонент
        return collider.CompareTag(mouseTag) ||
               collider.GetComponent<MouseCorpse>() != null ||
               collider.GetComponentInParent<MouseCorpse>() != null;
    }

    GameObject GetMouseRootObject(GameObject colliderObject)
    {
        // Если у самого объекта есть MouseCorpse - это корневой объект
        MouseCorpse mouseOnThis = colliderObject.GetComponent<MouseCorpse>();
        if (mouseOnThis != null)
            return colliderObject;

        // Ищем в родителях
        MouseCorpse mouseInParent = colliderObject.GetComponentInParent<MouseCorpse>();
        if (mouseInParent != null)
            return mouseInParent.gameObject;

        // Если не нашли - возвращаем сам объект
        return colliderObject;
    }

    void ProcessMouseCapture(GameObject mouseObject)
    {
        // Защита от повторной обработки одной мыши
        if (mouseObject == null || !mouseObject.activeInHierarchy)
            return;

        // Получаем компонент мыши
        MouseCorpse mouseCorpse = mouseObject.GetComponent<MouseCorpse>();

        if (mouseCorpse != null)
        {
            mouseCorpse.DestroyCorpse();
        }
        else
        {
            Destroy(mouseObject);
        }

        PlayCaptureEffects(mouseObject.transform.position);

        if (GameManager.Instance != null)
            GameManager.Instance.AddMouseCaught(scorePerMouse);
    }

    void PlayCaptureEffects(Vector3 position)
    {
        if (captureSound != null && audioSource != null)
            audioSource.PlayOneShot(captureSound);

        if (captureEffect != null)
            Instantiate(captureEffect, position, Quaternion.identity);
    }
}