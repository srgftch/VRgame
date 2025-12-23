using UnityEngine;
using System.Collections;

public class GlassCover : MonoBehaviour
{
    [Header("Break Settings")]
    [SerializeField] private float breakForceThreshold = 8f;
    [SerializeField] private float velocityBreakThreshold = 3f;

    [Header("Fragment References")]
    [SerializeField] private GameObject brokenGlassContainer; // Контейнер с заранее размещенными осколками
    [SerializeField] private bool autoFindFragments = true; // Автоматически искать осколки в дочерних объектах

    [Header("Effects")]
    [SerializeField] private AudioClip breakSound;
    [SerializeField] private ParticleSystem breakParticles;
    [SerializeField] private Material crackedMaterial;
    [SerializeField] private float crackShowTime = 0.1f; // Время показа трещин перед разрушением

    [Header("Fragment Physics")]
    [SerializeField] private float fragmentGravityMultiplier = 2f;
    [SerializeField] private float fragmentDownwardForce = 2f;
    [SerializeField] private float fragmentRandomForce = 0.2f;
    [SerializeField] private bool enableFragmentPhysics = true;

    // Кэшированные компоненты
    private Rigidbody[] fragmentRigidbodies;
    private Transform[] fragmentTransforms;
    private Vector3[] fragmentOriginalPositions;
    private Quaternion[] fragmentOriginalRotations;

    private bool isBroken = false;
    private AudioSource audioSource;
    private MeshRenderer meshRenderer;
    private Collider glassCollider;
    private Material originalMaterial;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        glassCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        if (meshRenderer != null)
        {
            originalMaterial = meshRenderer.material;
        }

        InitializeFragments();

        // Убедимся, что осколки выключены при старте
        if (brokenGlassContainer != null)
        {
            brokenGlassContainer.SetActive(false);
        }
    }

    void InitializeFragments()
    {
        // Если контейнер не назначен, ищем в дочерних объектах
        if (brokenGlassContainer == null && autoFindFragments)
        {
            brokenGlassContainer = FindOrCreateFragmentContainer();
        }

        if (brokenGlassContainer != null)
        {
            // Находим все Rigidbody в контейнере
            fragmentRigidbodies = brokenGlassContainer.GetComponentsInChildren<Rigidbody>(true);
            fragmentTransforms = new Transform[fragmentRigidbodies.Length];
            fragmentOriginalPositions = new Vector3[fragmentRigidbodies.Length];
            fragmentOriginalRotations = new Quaternion[fragmentRigidbodies.Length];

            // Сохраняем исходные позиции и повороты
            for (int i = 0; i < fragmentRigidbodies.Length; i++)
            {
                fragmentTransforms[i] = fragmentRigidbodies[i].transform;
                fragmentOriginalPositions[i] = fragmentTransforms[i].localPosition;
                fragmentOriginalRotations[i] = fragmentTransforms[i].localRotation;

                // Настраиваем Rigidbody
                if (enableFragmentPhysics)
                {
                    fragmentRigidbodies[i].useGravity = true;
                    fragmentRigidbodies[i].isKinematic = false;
                    fragmentRigidbodies[i].collisionDetectionMode = CollisionDetectionMode.Continuous;
                    fragmentRigidbodies[i].maxAngularVelocity = 20f;
                }
                else
                {
                    fragmentRigidbodies[i].isKinematic = true;
                }
            }

            Debug.Log($"Найдено осколков: {fragmentRigidbodies.Length}");
        }
        else
        {
            Debug.LogWarning("Не найден контейнер с осколками!");
            fragmentRigidbodies = new Rigidbody[0];
        }
    }

    GameObject FindOrCreateFragmentContainer()
    {
        // Ищем контейнер среди дочерних объектов
        foreach (Transform child in transform)
        {
            if (child.name.Contains("Fragments") || child.name.Contains("Broken") || child.name.Contains("Oсколки"))
            {
                return child.gameObject;
            }
        }

        // Создаем новый контейнер, если не нашли
        GameObject container = new GameObject("GlassFragments");
        container.transform.SetParent(transform);
        container.transform.localPosition = Vector3.zero;
        container.transform.localRotation = Quaternion.identity;

        return container;
    }

    public void HandleCrowbarImpact(Collision collision, float impactForce)
    {
        if (isBroken) return;

        Debug.Log($"Удар ломом! Сила: {impactForce}, Порог: {breakForceThreshold}");

        if (impactForce >= breakForceThreshold)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            Vector3 hitNormal = collision.contacts[0].normal;

            // Показываем трещины перед разрушением
            StartCoroutine(ShowCracksAndBreak(hitPoint, hitNormal));
        }
        else if (impactForce >= breakForceThreshold * 0.5f)
        {
            ShowCrackEffect(collision.contacts[0].point);
        }
    }

    IEnumerator ShowCracksAndBreak(Vector3 hitPoint, Vector3 hitNormal)
    {
        // Показываем материал с трещинами
        if (crackedMaterial != null && meshRenderer != null)
        {
            meshRenderer.material = crackedMaterial;
            yield return new WaitForSeconds(crackShowTime);
        }

        BreakGlass(hitPoint, hitNormal);
    }

    private void ShowCrackEffect(Vector3 hitPoint)
    {
        if (audioSource != null && breakSound != null)
        {
            audioSource.PlayOneShot(breakSound, 0.3f);
        }

        // Можно добавить эффект частиц для трещин
        if (breakParticles != null)
        {
            ParticleSystem particles = Instantiate(breakParticles, hitPoint, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, 2f);
        }
    }

    public void BreakGlass(Vector3 breakPoint, Vector3 breakDirection)
    {
        if (isBroken) return;

        isBroken = true;
        Debug.Log("Стекло разбито!");

        // Отключаем коллайдер целого стекла
        if (glassCollider != null)
        {
            glassCollider.enabled = false;
        }

        // Звук разбития
        if (breakSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(breakSound);
        }

        // Эффект частиц
        if (breakParticles != null)
        {
            breakParticles.transform.position = breakPoint;
            breakParticles.transform.forward = -breakDirection;
            breakParticles.Play();
        }

        // Активируем и настраиваем осколки
        ActivateFragments(breakPoint);

        // Скрываем целое стекло
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        // Отключаем скрипт
        enabled = false;

        // Уничтожаем целое стекло через время
        Destroy(gameObject, 5f);
    }

    void ActivateFragments(Vector3 breakPoint)
    {
        if (brokenGlassContainer == null || fragmentRigidbodies.Length == 0)
        {
            Debug.LogError("Нет осколков для активации!");
            return;
        }

        // Активируем контейнер с осколками
        brokenGlassContainer.SetActive(true);
        brokenGlassContainer.transform.SetParent(null); // Отделяем от родителя

        // Применяем физику к каждому осколку
        for (int i = 0; i < fragmentRigidbodies.Length; i++)
        {
            if (fragmentRigidbodies[i] == null) continue;

            Transform fragmentTransform = fragmentTransforms[i];

            // Включаем физику
            fragmentRigidbodies[i].isKinematic = false;
            fragmentRigidbodies[i].useGravity = true;

            // Рассчитываем силу для осколка
            Vector3 fragmentWorldPos = fragmentTransform.position;
            Vector3 forceDirection = Vector3.down; // Основная сила вниз

            // Небольшое отклонение от точки удара
            Vector3 fromBreakPoint = (fragmentWorldPos - breakPoint).normalized;
            forceDirection += fromBreakPoint * 0.1f; // Очень слабое разлетание

            // Добавляем случайность
            Vector3 randomForce = new Vector3(
                Random.Range(-fragmentRandomForce, fragmentRandomForce),
                0,
                Random.Range(-fragmentRandomForce, fragmentRandomForce)
            );

            // Итоговая сила
            Vector3 totalForce = (forceDirection * fragmentDownwardForce + randomForce) *
                                (fragmentGravityMultiplier - 1f);

            // Применяем силу
            fragmentRigidbodies[i].AddForce(totalForce, ForceMode.Impulse);

            // Добавляем небольшое вращение
            if (Random.value > 0.5f)
            {
                fragmentRigidbodies[i].AddTorque(
                    Random.Range(-fragmentRandomForce, fragmentRandomForce),
                    Random.Range(-fragmentRandomForce, fragmentRandomForce),
                    Random.Range(-fragmentRandomForce, fragmentRandomForce),
                    ForceMode.Impulse
                );
            }
        }

        // Автоочистка осколков через время
        StartCoroutine(CleanupFragments());
    }

    IEnumerator CleanupFragments()
    {
        yield return new WaitForSeconds(10f); // Ждем 10 секунд

        // Постепенно уничтожаем осколки
        if (brokenGlassContainer != null)
        {
            // Можно добавить эффект исчезновения
            Destroy(brokenGlassContainer, 2f);
        }
    }

    // Для отладки: сброс осколков в исходное состояние
    [ContextMenu("Reset Fragments")]
    public void ResetFragments()
    {
        if (brokenGlassContainer != null && fragmentRigidbodies != null)
        {
            for (int i = 0; i < fragmentRigidbodies.Length; i++)
            {
                if (fragmentRigidbodies[i] != null && fragmentTransforms[i] != null)
                {
                    fragmentRigidbodies[i].isKinematic = true;
                    fragmentRigidbodies[i].velocity = Vector3.zero;
                    fragmentRigidbodies[i].angularVelocity = Vector3.zero;

                    fragmentTransforms[i].localPosition = fragmentOriginalPositions[i];
                    fragmentTransforms[i].localRotation = fragmentOriginalRotations[i];
                }
            }

            brokenGlassContainer.SetActive(false);
            brokenGlassContainer.transform.SetParent(transform);
            brokenGlassContainer.transform.localPosition = Vector3.zero;
            brokenGlassContainer.transform.localRotation = Quaternion.identity;
        }

        isBroken = false;

        if (meshRenderer != null)
        {
            meshRenderer.enabled = true;
            if (originalMaterial != null)
            {
                meshRenderer.material = originalMaterial;
            }
        }

        if (glassCollider != null)
        {
            glassCollider.enabled = true;
        }

        enabled = true;
    }

    [ContextMenu("Break Glass")]
    public void ForceBreak()
    {
        BreakGlass(transform.position + Vector3.forward * 0.1f, Vector3.forward);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying && brokenGlassContainer != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(brokenGlassContainer.transform.position, Vector3.one * 0.2f);
            Gizmos.DrawIcon(brokenGlassContainer.transform.position, "d_Prefab Icon", true);
        }
    }
}