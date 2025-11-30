using UnityEngine;
using System.Collections;

public class RatNavigation : MonoBehaviour
{
    [Header("Navigation Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float stoppingDistance = 0.5f;

    [Header("Target Settings")]
    public string[] targetTags = { "Screwdriver", "Crowbar" };
    public float detectionRange = 10f;

    [Header("Physics Settings")]
    public float obstacleCheckDistance = 2f;
    public LayerMask obstacleLayerMask = 1;

    [Header("Carry Settings")]
    public Transform carryPoint;
    public Vector3 carryOffset = new Vector3(0, 0.3f, 0);
    public float maxCarrySize = 0.5f;

    [Header("Stun Settings")]
    public float stunDuration = 4f; // Длительность оглушения в секундах
    public ParticleSystem stunEffect; // Эффект оглушения
    public AudioClip stunSound; // Звук оглушения

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private Transform currentTarget;
    private Rigidbody rb;
    private bool isAvoiding = false;
    private float avoidTimer = 0f;
    private bool hasReachedTarget = false;
    private GameObject carriedObject;
    private Vector3 originalScale;

    // Переменные для оглушения
    private bool isStunned = false;
    private float stunTimer = 0f;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        if (carryPoint == null)
        {
            CreateCarryPoint();
        }

        StartCoroutine(FindToolsRoutine());
    }

    void CreateCarryPoint()
    {
        GameObject carryPointObj = new GameObject("CarryPoint");
        carryPointObj.transform.SetParent(transform);

        // Позиционируем точку переноски ближе к телу мыши
        // Уменьшаем высоту и добавляем небольшое смещение вперед
        carryPointObj.transform.localPosition = new Vector3(0, 0.2f, 0.1f);
        carryPoint = carryPointObj.transform;
    }

    void FixedUpdate()
    {
        // Обработка оглушения
        if (isStunned)
        {
            stunTimer -= Time.fixedDeltaTime;

            // Останавливаем движение при оглушении
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }

            // Проверяем, закончилось ли оглушение
            if (stunTimer <= 0f)
            {
                EndStun();
            }

            return; // Прерываем выполнение, пока мышь оглушена
        }

        if (hasReachedTarget)
        {
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }
            return;
        }

        if (hasTarget && currentTarget != null)
        {
            MoveToTarget(currentTarget.position);

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget <= stoppingDistance)
            {
                OnReachedTarget();
            }
        }
        else
        {
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }
        }

        if (isAvoiding)
        {
            avoidTimer -= Time.fixedDeltaTime;
            if (avoidTimer <= 0f)
            {
                isAvoiding = false;
            }
        }
    }

    IEnumerator FindToolsRoutine()
    {
        while (true)
        {
            if (!hasReachedTarget && !isStunned)
            {
                FindNearestTool();
            }
            yield return new WaitForSeconds(2f);
        }
    }

    void FindNearestTool()
    {
        Transform nearestTool = null;
        float nearestDistance = Mathf.Infinity;

        foreach (string tag in targetTags)
        {
            GameObject[] tools = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject tool in tools)
            {
                if (tool.activeInHierarchy && tool.GetComponent<Collider>() != null &&
                    tool.transform.parent == null)
                {
                    float distance = Vector3.Distance(transform.position, tool.transform.position);
                    if (distance < detectionRange && distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestTool = tool.transform;
                    }
                }
            }
        }

        if (nearestTool != null)
        {
            currentTarget = nearestTool;
            hasTarget = true;
            Debug.Log($"Мышь нашла инструмент: {currentTarget.name}, расстояние: {nearestDistance:F2}");
        }
        else
        {
            currentTarget = null;
            hasTarget = false;
        }
    }

    void MoveToTarget(Vector3 targetPos)
    {
        if (isAvoiding) return;

        Vector3 direction = (targetPos - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        if (rb != null)
        {
            Vector3 moveDirection = transform.forward * moveSpeed;
            rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);
        }

        CheckForObstacles();
    }

    void CheckForObstacles()
    {
        RaycastHit hit;

        Vector3[] rayDirections = {
            transform.forward,
            transform.forward + transform.right * 0.3f,
            transform.forward - transform.right * 0.3f
        };

        foreach (Vector3 dir in rayDirections)
        {
            if (Physics.Raycast(transform.position, dir, out hit, obstacleCheckDistance, obstacleLayerMask))
            {
                if (!IsTool(hit.collider.gameObject))
                {
                    AvoidObstacle(hit.normal);
                    return;
                }
            }
        }
    }

    void AvoidObstacle(Vector3 obstacleNormal)
    {
        isAvoiding = true;
        avoidTimer = 1f;

        Vector3 avoidDirection = Vector3.Reflect(transform.forward, obstacleNormal).normalized;

        Quaternion avoidRotation = Quaternion.LookRotation(avoidDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, avoidRotation, rotationSpeed * 2f * Time.fixedDeltaTime);

        if (rb != null)
        {
            Vector3 moveDirection = transform.forward * moveSpeed * 0.5f;
            rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);
        }
    }

    bool IsTool(GameObject obj)
    {
        foreach (string tag in targetTags)
        {
            if (obj.CompareTag(tag)) return true;
        }
        return false;
    }

    void OnReachedTarget()
    {
        if (currentTarget != null)
        {
            Debug.Log($"Мышь достигла {currentTarget.name} и поднимает его");

            PickUpObject(currentTarget.gameObject);

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
            }

            hasReachedTarget = true;
            hasTarget = false;

            PlaySuccessEffects();
        }
    }

    void PickUpObject(GameObject obj)
    {
        carriedObject = obj;
        originalScale = obj.transform.localScale;

        Rigidbody toolRb = obj.GetComponent<Rigidbody>();
        if (toolRb != null)
        {
            toolRb.isKinematic = true;
            toolRb.useGravity = false;
        }

        Collider toolCollider = obj.GetComponent<Collider>();
        if (toolCollider != null)
        {
            toolCollider.enabled = false;
        }

        MonoBehaviour[] interactables = obj.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour interactable in interactables)
        {
            if (interactable.GetType().Name.Contains("Interactable"))
            {
                interactable.enabled = false;
            }
        }

        // Устанавливаем родителя, но НЕ используем локальную позицию
        obj.transform.SetParent(carryPoint);

        // Сбрасываем локальную позицию к нулю, чтобы объект был точно в точке carryPoint
        obj.transform.localPosition = Vector3.zero;

        // Применяем смещение относительно точки переноски
        obj.transform.localPosition = carryOffset;

        // Сбрасываем поворот
        obj.transform.localRotation = Quaternion.identity;

        Debug.Log($"Инструмент {obj.name} поднят. Позиция: {obj.transform.position}, локальная позиция: {obj.transform.localPosition}");
    }

    public void DropObject()
    {
        if (carriedObject != null)
        {
            // Сохраняем текущую позицию инструмента (на спине мыши)
            Vector3 dropPosition = carriedObject.transform.position;

            Rigidbody toolRb = carriedObject.GetComponent<Rigidbody>();
            if (toolRb != null)
            {
                toolRb.isKinematic = false;
                toolRb.useGravity = true;

                // Сбрасываем скорость, чтобы предмет падал естественно
                toolRb.velocity = Vector3.zero;
                toolRb.angularVelocity = Vector3.zero;
            }

            Collider toolCollider = carriedObject.GetComponent<Collider>();
            if (toolCollider != null)
            {
                toolCollider.enabled = true;
            }

            MonoBehaviour[] interactables = carriedObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour interactable in interactables)
            {
                if (interactable.GetType().Name.Contains("Interactable"))
                {
                    interactable.enabled = true;
                }
            }

            // Отключаем родителя, оставляя объект на текущей позиции
            carriedObject.transform.SetParent(null);

            // Устанавливаем позицию сброса - там же, где он был на спине мыши
            carriedObject.transform.position = dropPosition;

            Debug.Log($"Инструмент {carriedObject.name} сброшен с позиции: {dropPosition}");
            carriedObject = null;
        }
    }

    // Метод для оглушения мыши
    public void Stun(float duration = 0f)
    {
        if (isStunned) return; // Уже оглушена

        // Если длительность не указана, используем значение по умолчанию
        if (duration <= 0f)
        {
            duration = stunDuration;
        }

        isStunned = true;
        stunTimer = duration;

        // Останавливаем движение
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }

        // Сбрасываем цели
        hasTarget = false;
        hasReachedTarget = false;

        // Если несла инструмент - сбрасываем его
        if (carriedObject != null)
        {
            DropObject();
        }

        // Запускаем эффекты оглушения
        PlayStunEffects();

        Debug.Log($"Мышь оглушена на {duration} секунд!");
    }

    void EndStun()
    {
        isStunned = false;
        stunTimer = 0f;

        // Останавливаем эффекты оглушения
        if (stunEffect != null)
        {
            stunEffect.Stop();
        }

        Debug.Log("Мышь пришла в себя!");
    }

    void PlaySuccessEffects()
    {
        // Эффекты при поднятии инструмента
    }

    void PlayStunEffects()
    {
        // Звук оглушения
        if (stunSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(stunSound);
        }

        // Визуальный эффект оглушения
        if (stunEffect != null)
        {
            stunEffect.Play();
        }
    }

    // Обработка столкновения с ломом
    void OnCollisionEnter(Collision collision)
    {
        // Проверяем, что столкнулись с ломом
        if (collision.gameObject.CompareTag("Crowbar"))
        {
            // Оглушаем мышь
            Stun();
        }
    }

    // Альтернативный вариант с триггером (если коллайдер лома - триггер)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Crowbar"))
        {
            Stun();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (hasTarget && currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(currentTarget.position, stoppingDistance);
        }

        Gizmos.color = Color.blue;
        Vector3[] rayDirections = {
            transform.forward,
            transform.forward + transform.right * 0.3f,
            transform.forward - transform.right * 0.3f
        };

        foreach (Vector3 dir in rayDirections)
        {
            Gizmos.DrawRay(transform.position, dir * obstacleCheckDistance);
        }

        if (hasReachedTarget)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }

        if (carryPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(carryPoint.position, Vector3.one * 0.1f);
        }

        // Показываем состояние оглушения
        if (isStunned)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.7f);
        }
    }
}