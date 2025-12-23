using UnityEngine;

public class SelfControlledWrench : MonoBehaviour
{
//    [Header("Wrench Settings")]
//    public float boltSnapDistance = 0.1f;
//    public float rotationSpeed = 100f;
//    public Transform boltSocket;

//    [Header("Input Settings")]
//    public string triggerButton = "XRI_Right_Trigger";
//    public KeyCode testKey = KeyCode.Space;
//    public bool useVRInput = true;
//    public float inputCheckInterval = 0.1f;

//    [Header("Bolt Connection")]
//    public BoltController currentBolt;
//    public bool isAttachedToBolt = false;

//    private bool isRotating = false;
//    private Rigidbody rb;
//    private Transform originalParent;
//    private Vector3 originalPosition;
//    private Quaternion originalRotation;
//    private float lastInputCheckTime = 0f;
//    private bool isGrabbed = false; // Добавили флаг захвата

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        SaveOriginalTransform();

//        // Автоматически находим XR компоненты
//        AutoSetupXR();
//    }

//    void Update()
//    {
//        // Проверяем ввод с интервалом
//        //if (Time.time - lastInputCheckTime > inputCheckInterval)
//        //{
//        //    CheckInput();
//        //    lastInputCheckTime = Time.time;
//        //}
//        CheckInput();
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            Debug.Log("ПРОБЕЛ НАЖАТ! Пробуем вращать...");
            
//        }

//        if (Input.GetKeyUp(KeyCode.Space))
//        {
//            Debug.Log("ПРОБЕЛ ОТПУЩЕН! Останавливаем...");
            
//        }
//        // Вращение
//        if (isRotating && currentBolt != null)
//        {
//            RotateWrench();
//        }

//        // Автоматический поиск болтов, если не прикреплен и не в руках
//        if (!isAttachedToBolt && !isGrabbed)
//        {
//            AutoFindAndSnapToBolt();
//        }
//    }

//    void FixedUpdate()
//    {
//        // Если ключ не в руках и не прикреплен - ищем болты
//        if (!isGrabbed && !isAttachedToBolt)
//        {
//            CheckForNearbyBolts();
//        }
//    }

//    void CheckInput()
//    {
//        bool shouldRotate = false;

//        if (useVRInput && !string.IsNullOrEmpty(triggerButton))
//        {
//            shouldRotate = Input.GetButton(triggerButton);
//            // Также пробуем альтернативные имена кнопок
//            if (!shouldRotate) shouldRotate = Input.GetButton("Fire1");
//            if (!shouldRotate) shouldRotate = Input.GetButton("Submit");
//        }
//        else
//        {
//            shouldRotate = Input.GetKey(testKey);
//        }

//        // Отладочный вывод
//        if (shouldRotate)
//        {
//            Debug.Log($"Кнопка нажата. Прикреплен к болту: {isAttachedToBolt}, текущий болт: {currentBolt != null}");
//        }

//        // Управление вращением
//        if (shouldRotate && !isRotating && isAttachedToBolt && currentBolt != null)
//        {
//            Debug.Log("Пытаемся начать вращение...");
//            StartRotation();
//        }
//        else if (!shouldRotate && isRotating)
//        {
//            Debug.Log("Останавливаем вращение...");
//            StopRotation();
//        }
//    }

//    void AutoSetupXR()
//    {
//        // Попробуем автоматически настроить для XR
//#if UNITY_XR_INTERACTION_TOOLKIT
//        var grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable>();
//        if (grabInteractable != null)
//        {
//            grabInteractable.selectEntered.AddListener((args) => OnGrabbed());
//            grabInteractable.selectExited.AddListener((args) => OnReleased());
//            Debug.Log("XR Grab Interactable найден и настроен");
//        }
//#endif
//    }

//    void SaveOriginalTransform()
//    {
//        originalPosition = transform.position;
//        originalRotation = transform.rotation;
//        originalParent = transform.parent;
//    }

//    void AutoFindAndSnapToBolt()
//    {
//        BoltController nearestBolt = FindNearestBolt();

//        if (nearestBolt != null)
//        {
//            float distance = Vector3.Distance(transform.position, nearestBolt.wrenchSocket.position);

//            if (distance <= boltSnapDistance && !nearestBolt.isUnscrewed)
//            {
//                AttachToBolt(nearestBolt);
//            }
//        }
//    }

//    void CheckForNearbyBolts()
//    {
//        BoltController nearestBolt = FindNearestBolt();

//        if (nearestBolt != null)
//        {
//            float distance = Vector3.Distance(transform.position, nearestBolt.wrenchSocket.position);

//            if (distance <= boltSnapDistance * 2f && !nearestBolt.isUnscrewed)
//            {
//                AttachToBolt(nearestBolt);
//            }
//        }
//    }

//    BoltController FindNearestBolt()
//    {
//        BoltController nearestBolt = null;
//        float nearestDistance = Mathf.Infinity;

//        GameObject[] bolts = GameObject.FindGameObjectsWithTag("Bolt");

//        foreach (GameObject boltObj in bolts)
//        {
//            BoltController bolt = boltObj.GetComponent<BoltController>();
//            if (bolt != null && !bolt.isUnscrewed)
//            {
//                float distance = Vector3.Distance(transform.position, bolt.wrenchSocket.position);
//                if (distance < nearestDistance)
//                {
//                    nearestDistance = distance;
//                    nearestBolt = bolt;
//                }
//            }
//        }

//        return nearestBolt;
//    }

//    public void AttachToBolt(BoltController bolt)
//    {
//        currentBolt = bolt;
//        isAttachedToBolt = true;

//        // Отключаем физику
//        if (rb != null)
//        {
//            rb.isKinematic = true;
//            rb.useGravity = false;
//        }

//        // Прикрепляем к болту
//        if (bolt.wrenchSocket != null)
//        {
//            transform.position = bolt.wrenchSocket.position;
//            transform.rotation = bolt.wrenchSocket.rotation;
//            transform.SetParent(bolt.wrenchSocket);
//        }
//        else
//        {
//            transform.SetParent(bolt.transform);
//        }

//        // Уведомляем болт
//        bolt.OnWrenchAttached(this);

//        Debug.Log($"Гаечный ключ прикреплен к болту: {bolt.name}");
//    }

//    public void DetachFromBolt()
//    {
//        if (currentBolt != null)
//        {
//            currentBolt.OnWrenchDetached();
//            currentBolt = null;
//        }

//        isAttachedToBolt = false;
//        isRotating = false;

//        // Возвращаем физику
//        if (rb != null)
//        {
//            rb.isKinematic = false;
//            rb.useGravity = true;
//        }

//        // Возвращаем оригинальный parent
//        transform.SetParent(originalParent);

//        Debug.Log("Гаечный ключ откреплен от болта");
//    }

//    void StartRotation()
//    {
//        if (isAttachedToBolt && currentBolt != null && !currentBolt.isUnscrewed)
//        {
//            isRotating = true;
//            Debug.Log($"Начато вращение ключа. Болт: {currentBolt.name}, откручен: {currentBolt.isUnscrewed}");
//            currentBolt.StartUnscrewing();
//        }
//        else
//        {
//            Debug.LogWarning($"Не могу начать вращение. Прикреплен: {isAttachedToBolt}, болт: {currentBolt != null}, откручен: {(currentBolt != null ? currentBolt.isUnscrewed.ToString() : "N/A")}");
//        }
//    }

//    void StopRotation()
//    {
//        isRotating = false;

//        if (currentBolt != null)
//        {
//            currentBolt.StopUnscrewing();
//        }

//        Debug.Log("Остановлено вращение ключа");
//    }

//    void RotateWrench()
//    {
//        // Вращаем ключ вокруг оси болта
//        if (currentBolt != null)
//        {
//            // Вращаем сам ключ
//            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);

//            // Обновляем прогресс болта
//            currentBolt.UpdateUnscrewProgress(rotationSpeed * Time.deltaTime);

//            Debug.Log($"Вращение ключа. Прогресс: {currentBolt.currentUnscrewAngle}/{currentBolt.unscrewAngle}");
//        }
//    }

//    // Для кнопок UI
//    public void OnButtonPressed() => StartRotation();
//    public void OnButtonReleased() => StopRotation();

//    // Для системы захвата XR
//    public void OnGrabbed()
//    {
//        isGrabbed = true;
//        Debug.Log("Ключ взят в руку");

//        // Если был прикреплен к болту - открепляем
//        if (isAttachedToBolt)
//        {
//            DetachFromBolt();
//        }
//    }

//    public void OnReleased()
//    {
//        isGrabbed = false;
//        Debug.Log("Ключ отпущен");

//        // После отпускания проверяем болты рядом
//        CheckForNearbyBolts();
//    }

//    // Публичные методы для тестирования
//    public void TestStartRotation() => StartRotation();
//    public void TestStopRotation() => StopRotation();

//    void OnDrawGizmosSelected()
//    {
//        // Визуализация дистанции прилипания
//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(transform.position, boltSnapDistance);

//        // Линия к текущему болту
//        if (currentBolt != null && currentBolt.wrenchSocket != null)
//        {
//            Gizmos.color = Color.green;
//            Gizmos.DrawLine(transform.position, currentBolt.wrenchSocket.position);
//        }
//    }
}