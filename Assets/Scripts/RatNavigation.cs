using UnityEngine;
using System.Collections;

public class RatNavigation : MonoBehaviour
{
    [Header("Navigation Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 20f;
    public float stoppingDistance = 0.1f;

    [Header("Target Settings")]
    public string[] targetTags = { "Screwdriver", "Wrench", "Key" };
    public float detectionRange = 10f;

    [Header("Return Settings")]
    [SerializeField] private GameObject _returnPointObject;
    public Vector3 returnPosition = Vector3.zero;
    public bool useReturnPoint = true;
    public float returnStoppingDistance = 1f;
    public bool destroyAfterReturn = true; // НОВОЕ: уничтожать мышь после возврата
    public float destroyDelay = 0.5f; // Задержка перед уничтожением
    [SerializeField] private GameObject corpsePrefab; // Укажите префаб трупа мыши в инспекторе
    private int stunCount = 0;

    [Header("Obstacle Avoidance Settings")]
    public float obstacleCheckDistance = 1.5f;
    public LayerMask obstacleLayerMask = 1;
    public float sideRayOffset = 0.3f;
    public float avoidRotationSpeed = 30f;
    public float minDistanceToObstacle = 0.5f;

    [Header("Carry Settings")]
    public Transform carryPoint;
    public Vector3 carryOffset = new Vector3(0, 0.3f, 0);

    [Header("Stun Settings")]
    public float defaultStunDuration = 4f;
    public ParticleSystem defaultStunEffect;
    public AudioClip defaultStunSound;

    private bool hasTarget = false;
    private Transform currentTarget;
    private Rigidbody rb;
    private bool hasReachedTarget = false;
    private GameObject carriedObject;

    // Новые переменные для системы возврата
    private bool isReturningHome = false;
    private bool hasReturnedHome = false;
    private Vector3 homePosition;

    // Переменные для оглушения
    private bool isStunned = false;
    private float stunTimer = 0f;
    private AudioSource audioSource;

    // Переменные для поиска
    private float nextSearchTime = 0f;
    public float searchInterval = 0.3f;

    // Переменные для обхода препятствий
    private bool isAvoiding = false;
    private Vector3 avoidDirection;
    private float avoidTimer = 0f;
    private const float MAX_AVOID_TIME = 3f;

    [SerializeField] private GameObject transformationPrefab;

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

        // Инициализация точки возврата
        InitializeReturnPoint();

        // Ищем первую цель сразу
        FindNearestTool();
        nextSearchTime = Time.time + searchInterval;
    }

    void InitializeReturnPoint()
    {
        if (useReturnPoint)
        {
            if (_returnPointObject != null && _returnPointObject.transform != null)
            {
                homePosition = _returnPointObject.transform.position;
            }
            else
            {
                homePosition = returnPosition;

                if (homePosition == Vector3.zero)
                {
                    homePosition = transform.position;
                }
            }
        }
    }

    public Transform returnPoint
    {
        get
        {
            if (_returnPointObject != null)
                return _returnPointObject.transform;
            return null;
        }
        set
        {
            if (value != null)
                _returnPointObject = value.gameObject;
            else
                _returnPointObject = null;
        }
    }
    void CreateCarryPoint()
    {
        GameObject carryPointObj = new GameObject("CarryPoint");
        carryPointObj.transform.SetParent(transform);
        carryPointObj.transform.localPosition = new Vector3(0, 0.2f, 0.1f);
        carryPoint = carryPointObj.transform;
    }

    void Update()
    {
        if (!hasReachedTarget && !hasTarget && !isStunned && !isReturningHome)
        {
            if (Time.time > nextSearchTime)
            {
                FindNearestTool();
                nextSearchTime = Time.time + searchInterval;
            }
        }

        if (isAvoiding)
        {
            avoidTimer += Time.deltaTime;
            if (avoidTimer > MAX_AVOID_TIME)
            {
                isAvoiding = false;
                avoidTimer = 0f;
            }
        }
    }

    void FixedUpdate()
    {
        if (isStunned)
        {
            stunTimer -= Time.fixedDeltaTime;
            if (rb != null) rb.velocity = Vector3.zero;

            if (stunTimer <= 0f) EndStun();
            return;
        }

        // Если вернулись домой и уничтожились - ничего не делаем
        if (hasReturnedHome) return;

        if (isReturningHome && carriedObject != null)
        {
            MoveToHome();
            return;
        }

        if (hasReachedTarget)
        {
            if (rb != null) rb.velocity = Vector3.zero;
            return;
        }

        if (hasTarget && currentTarget != null)
        {
            MoveToTargetDirect();

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget <= stoppingDistance)
            {
                OnReachedTarget();
            }
        }
        else
        {
            if (rb != null) rb.velocity = Vector3.zero;
        }
    }

    void MoveToTargetDirect()
    {
        if (currentTarget == null) return;

        Vector3 targetPos = currentTarget.position;

        if (isAvoiding)
        {
            MoveInAvoidDirection();
            return;
        }

        if (CheckForObstaclesExcludingTarget())
        {
            StartAvoidance();
            return;
        }

        MoveDirectlyToTarget(targetPos);
    }

    void MoveDirectlyToTarget(Vector3 targetPos)
    {
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
    }

    bool CheckForObstaclesExcludingTarget()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleCheckDistance, obstacleLayerMask))
        {
            if (!IsTargetOrCarried(hit.collider.gameObject) && hit.distance < minDistanceToObstacle)
            {
                return true;
            }
        }

        Vector3[] rayDirections = {
            transform.forward + transform.right * sideRayOffset,
            transform.forward - transform.right * sideRayOffset
        };

        foreach (Vector3 dir in rayDirections)
        {
            if (Physics.Raycast(transform.position, dir, out hit, obstacleCheckDistance * 0.7f, obstacleLayerMask))
            {
                if (!IsTargetOrCarried(hit.collider.gameObject) && hit.distance < minDistanceToObstacle)
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool IsTargetOrCarried(GameObject obj)
    {
        if (currentTarget != null && obj.transform == currentTarget)
            return true;

        if (carriedObject != null && obj == carriedObject)
            return true;

        foreach (string tag in targetTags)
        {
            if (obj.CompareTag(tag))
                return true;
        }

        return false;
    }

    void StartAvoidance()
    {
        isAvoiding = true;
        avoidTimer = 0f;

        RaycastHit hitRight, hitLeft;
        bool rightBlocked = CheckDirectionForObstacle(transform.right, 1f);
        bool leftBlocked = CheckDirectionForObstacle(-transform.right, 1f);

        if (!rightBlocked && !leftBlocked)
        {
            Vector3 toTarget = (currentTarget.position - transform.position).normalized;
            float dotRight = Vector3.Dot(transform.right, toTarget);
            avoidDirection = dotRight > 0 ? transform.right : -transform.right;
        }
        else if (!rightBlocked)
        {
            avoidDirection = transform.right;
        }
        else if (!leftBlocked)
        {
            avoidDirection = -transform.right;
        }
        else
        {
            avoidDirection = -transform.forward;
        }
    }

    bool CheckDirectionForObstacle(Vector3 direction, float distance)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, distance, obstacleLayerMask))
        {
            return !IsTargetOrCarried(hit.collider.gameObject);
        }
        return false;
    }

    void MoveInAvoidDirection()
    {
        if (avoidDirection != Vector3.zero)
        {
            Quaternion avoidRotation = Quaternion.LookRotation(avoidDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, avoidRotation, avoidRotationSpeed * Time.fixedDeltaTime);
        }

        if (rb != null)
        {
            Vector3 moveDirection = transform.forward * moveSpeed * 0.8f;
            rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);
        }

        if (currentTarget != null)
        {
            Vector3 toTarget = currentTarget.position - transform.position;
            float distanceToTarget = toTarget.magnitude;

            if (!CheckForObstaclesExcludingTarget() && distanceToTarget > stoppingDistance)
            {
                float angleToTarget = Vector3.Angle(transform.forward, toTarget.normalized);
                if (angleToTarget < 45f)
                {
                    isAvoiding = false;
                    avoidTimer = 0f;
                }
            }
        }
    }

    void MoveToHome()
    {
        float distanceToHome = Vector3.Distance(transform.position, homePosition);

        if (distanceToHome <= returnStoppingDistance)
        {
            OnReachedHome();
            return;
        }

        if (isAvoiding)
        {
            MoveInAvoidDirection();
        }
        else if (CheckForObstaclesExcludingTarget())
        {
            StartAvoidance();
        }
        else
        {
            MoveDirectlyToTarget(homePosition);
        }
    }

    // ИЗМЕНЕНО: Добавлено уничтожение мыши
    void OnReachedHome()
    {
        if (rb != null) rb.velocity = Vector3.zero;
        hasReturnedHome = true;
        isReturningHome = false;

        // Сначала сбрасываем предмет
        if (carriedObject != null)
        {
            DropObjectAtHome();
        }

        // Затем уничтожаем мышь, если включена опция
        if (destroyAfterReturn)
        {
            StartCoroutine(DestroyRat());
        }
    }

    // НОВЫЙ метод: уничтожение мыши с задержкой
    IEnumerator DestroyRat()
    {
        Debug.Log($"Мышь достигла точки возврата и будет уничтожена через {destroyDelay} сек");

        // Можно добавить эффекты перед уничтожением
        // Например, анимацию, звук, частицы

        yield return new WaitForSeconds(destroyDelay);

        // Уничтожаем объект мыши
        Destroy(gameObject);
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
        }
        else
        {
            currentTarget = null;
            hasTarget = false;
        }
    }

    void OnReachedTarget()
    {
        if (currentTarget != null)
        {
            PickUpObject(currentTarget.gameObject);

            if (rb != null) rb.velocity = Vector3.zero;
            hasReachedTarget = true;
            hasTarget = false;
            isAvoiding = false;

            if (useReturnPoint && carriedObject != null)
            {
                StartReturningHome();
            }
        }
    }

    void StartReturningHome()
    {
        if (!useReturnPoint) return;

        isReturningHome = true;
        hasReturnedHome = false;
        isAvoiding = false;
    }

    void PickUpObject(GameObject obj)
    {
        carriedObject = obj;

        Rigidbody toolRb = obj.GetComponent<Rigidbody>();
        if (toolRb != null)
        {
            toolRb.isKinematic = true;
            toolRb.useGravity = false;
        }

        Collider toolCollider = obj.GetComponent<Collider>();
        if (toolCollider != null) toolCollider.enabled = false;

        obj.transform.SetParent(carryPoint);
        obj.transform.localPosition = carryOffset;
        obj.transform.localRotation = Quaternion.identity;
    }

    void DropObjectAtHome()
    {
        if (carriedObject != null)
        {
            // Без рандомного смещения - кладем аккуратно
            Vector3 dropPosition = homePosition;

            // Проверяем землю под точкой
            RaycastHit hit;
            if (Physics.Raycast(dropPosition + Vector3.up * 2f, Vector3.down, out hit, 5f))
            {
                dropPosition = hit.point + Vector3.up * 0.05f; // Чуть выше земли
            }

            Rigidbody toolRb = carriedObject.GetComponent<Rigidbody>();
            if (toolRb != null)
            {
                toolRb.isKinematic = false;
                toolRb.useGravity = true;
                toolRb.velocity = Vector3.zero;
                toolRb.angularVelocity = Vector3.zero;
            }

            Collider toolCollider = carriedObject.GetComponent<Collider>();
            if (toolCollider != null) toolCollider.enabled = true;

            carriedObject.transform.SetParent(null);
            carriedObject.transform.position = dropPosition;
            carriedObject = null;
        }
    }

    public void DropObject()
    {
        if (carriedObject != null)
        {
            Vector3 dropPosition = carriedObject.transform.position;

            Rigidbody toolRb = carriedObject.GetComponent<Rigidbody>();
            if (toolRb != null)
            {
                toolRb.isKinematic = false;
                toolRb.useGravity = true;
                toolRb.velocity = Vector3.zero;
                toolRb.angularVelocity = Vector3.zero;
            }

            Collider toolCollider = carriedObject.GetComponent<Collider>();
            if (toolCollider != null) toolCollider.enabled = true;

            carriedObject.transform.SetParent(null);
            carriedObject.transform.position = dropPosition;
            carriedObject = null;

            if (isReturningHome)
            {
                isReturningHome = false;
                hasReturnedHome = false;
            }
        }
    }

    public void Stun(float duration = 0f, ParticleSystem customEffect = null, AudioClip customSound = null)
    {
        if (isStunned) return;

        stunCount++;

        // Если это второе оглушение — превращаем в труп
        if (stunCount >= 2)
        {
            TransformIntoCorpse();
            return; // Не продолжаем логику оглушения
        }

        // Иначе — обычное оглушение
        float finalDuration = duration > 0 ? duration : defaultStunDuration;
        isStunned = true;
        stunTimer = finalDuration;

        if (rb != null) rb.velocity = Vector3.zero;
        hasTarget = false;
        hasReachedTarget = false;
        isReturningHome = false;
        isAvoiding = false;

        if (carriedObject != null) DropObject();

        if (customEffect != null)
            StartCoroutine(PlayEffect(customEffect));
        else if (defaultStunEffect != null)
            defaultStunEffect.Play();

        if (customSound != null && audioSource != null)
            audioSource.PlayOneShot(customSound);
        else if (defaultStunSound != null && audioSource != null)
            audioSource.PlayOneShot(defaultStunSound);
    }

    void TransformIntoCorpse()
    {
        Debug.Log("Превращение в труп: prefab = " + (corpsePrefab != null ? "OK" : "NULL"));

        if (corpsePrefab != null)
        {
            GameObject corpse = Instantiate(corpsePrefab, transform.position, transform.rotation);
            Debug.Log("Труп создан на позиции: " + corpse.transform.position);

            Rigidbody corpseRb = corpse.GetComponent<Rigidbody>();
            if (corpseRb != null && rb != null)
            {
                corpseRb.velocity = rb.velocity;
            }
        }
        else
        {
            Debug.LogError("corpsePrefab не назначен! Труп не создан.");
        }

        Destroy(gameObject);
    }

    IEnumerator PlayEffect(ParticleSystem effect)
    {
        ParticleSystem instance = Instantiate(effect, transform.position, Quaternion.identity);
        instance.transform.SetParent(transform);
        yield return new WaitForSeconds(effect.main.duration);
        if (instance != null) Destroy(instance.gameObject);
    }

    void EndStun()
    {
        isStunned = false;
        stunTimer = 0f;
        if (defaultStunEffect != null) defaultStunEffect.Stop();
    }

    public void TransformIntoObject(GameObject newObjectPrefab = null, bool copyVelocity = false)
    {
        GameObject prefabToUse = newObjectPrefab != null ? newObjectPrefab : transformationPrefab;

        if (prefabToUse == null)
        {
            Debug.LogError("TransformIntoObject: No prefab specified!");
            return;
        }

        Vector3 position = transform.position;
        Quaternion rotation = transform.rotation;
        Vector3 velocity = Vector3.zero;

        if (rb != null && copyVelocity)
        {
            velocity = rb.velocity;
        }

        // Создаем новый объект
        GameObject newObject = Instantiate(prefabToUse, position, rotation);

        // Если нужно передать скорость
        if (copyVelocity)
        {
            Rigidbody newRb = newObject.GetComponent<Rigidbody>();
            if (newRb != null)
            {
                newRb.velocity = velocity;
            }
        }

        // Уничтожаем текущий объект
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * obstacleCheckDistance);
        Gizmos.DrawRay(transform.position, (transform.forward + transform.right * sideRayOffset) * obstacleCheckDistance * 0.7f);
        Gizmos.DrawRay(transform.position, (transform.forward - transform.right * sideRayOffset) * obstacleCheckDistance * 0.7f);

        if (useReturnPoint)
        {
            Gizmos.color = Color.magenta;
            Vector3 drawHomePosition = _returnPointObject != null ? _returnPointObject.transform.position :
                                      (returnPosition != Vector3.zero ? returnPosition : homePosition);
            Gizmos.DrawWireSphere(drawHomePosition, returnStoppingDistance);
        }

        if (hasTarget && currentTarget != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentTarget.position, 0.2f);
        }

        if (isAvoiding)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, avoidDirection * 1f);
        }

        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, minDistanceToObstacle);
    }
}