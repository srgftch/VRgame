using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Добавляем using для XR
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Key : MonoBehaviour
{
    public enum KeyType { Circle, Triangle, Square }
    public KeyType keyType = KeyType.Circle;

    [Header("Компоненты")]
    public MonoBehaviour grabComponent; // XRGrabInteractable или аналогичный

    [Header("Эффекты")]
    public AudioClip snapSound;

    [Header("Настройки физики")]
    public bool usePhysics = true;

    // Свойства
    public bool IsSnapped { get; private set; }
    private Rigidbody rb;
    private Collider[] colliders;
    private Transform originalParent;
    private XRBaseInteractable xrInteractable; // Ссылка на XR компонент
    private bool wasXRGrabbed = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        colliders = GetComponents<Collider>();
        originalParent = transform.parent;

        // Получаем XR компонент, если есть
        xrInteractable = GetComponent<XRBaseInteractable>();
        if (xrInteractable != null)
        {
            Debug.Log($"🎮 Ключ {keyType} имеет XR компонент: {xrInteractable.GetType().Name}");
        }

        if (usePhysics && rb == null)
        {
            Debug.LogWarning($"⚠️ Ключ {keyType} не имеет Rigidbody, но usePhysics = true", gameObject);
        }
    }

    private void Update()
    {
        // Отслеживаем, был ли ключ взят XR системой
        if (xrInteractable != null && !wasXRGrabbed)
        {
            wasXRGrabbed = xrInteractable.isSelected;
        }
    }

    public void SnapToPosition(Transform snapPoint)
    {
        if (IsSnapped)
        {
            Debug.LogWarning($"⚠️ Ключ {keyType} уже зафиксирован!", gameObject);
            return;
        }

        IsSnapped = true;
        Debug.Log($"🔐 Фиксируем ключ {keyType}...");

        // ОСОБЕННО ВАЖНО: Отключаем XR взаимодействие ПЕРЕД тем как делать kinematic
        DisableXRInteraction();

        // Отключаем взаимодействие
        if (grabComponent != null)
        {
            grabComponent.enabled = false;
            Debug.Log($"   Отключен {grabComponent.GetType().Name}");
        }

        // Отключаем коллайдеры
        foreach (Collider col in colliders)
        {
            if (col != null && col.enabled && !col.isTrigger)
            {
                col.enabled = false;
                Debug.Log($"   Отключен коллайдер: {col.GetType().Name}");
            }
        }

        // Запускаем плавную фиксацию
        StartCoroutine(SmoothSnap(snapPoint));

        // Звук
        if (snapSound != null)
        {
            AudioSource.PlayClipAtPoint(snapSound, transform.position, 0.5f);
            Debug.Log("   Воспроизведен звук фиксации");
        }
    }

    // Новый метод для отключения XR взаимодействия
    private void DisableXRInteraction()
    {
        if (xrInteractable != null)
        {
            // 1. Принудительно отпускаем ключ, если он взят
            if (xrInteractable.isSelected)
            {
                Debug.Log($"   Принудительно отпускаем XR ключ {keyType}");

                // Получаем контроллер, который держит ключ
                var interactor = xrInteractable.interactorsSelecting[0];
                if (interactor != null)
                {
                    // Отключаем взаимодействие
                    xrInteractable.interactionManager.SelectExit(
                        interactor as IXRSelectInteractor,
                        xrInteractable
                    );
                }
            }

            // 2. Отключаем компонент
            xrInteractable.enabled = false;
            Debug.Log($"   Отключен XR компонент: {xrInteractable.GetType().Name}");

            // 3. Ждем один кадр, чтобы XR система обновила состояние
            StartCoroutine(DelayPhysicsDisable());
        }
        else
        {
            // Если нет XR компонента, просто отключаем физику
            DisablePhysicsImmediate();
        }
    }

    // Отключаем физику с задержкой в 1 кадр
    private IEnumerator DelayPhysicsDisable()
    {
        yield return null; // Ждем один кадр

        // Теперь безопасно отключаем физику
        DisablePhysicsImmediate();
    }

    // Метод для отключения физики
    private void DisablePhysicsImmediate()
    {
        if (rb != null)
        {
            // Сохраняем предыдущее состояние
            bool wasKinematic = rb.isKinematic;

            // Останавливаем движение
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Делаем kinematic
            rb.isKinematic = true;

            Debug.Log($"   Rigidbody: wasKinematic={wasKinematic}, now={rb.isKinematic}");
        }
    }

    private IEnumerator SmoothSnap(Transform snapPoint)
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float duration = 0.3f; // Немного увеличили длительность
        float elapsed = 0f;

        Debug.Log($"   Начало плавной фиксации за {duration} сек");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t); // Smooth step

            transform.position = Vector3.Lerp(startPos, snapPoint.position, t);
            transform.rotation = Quaternion.Slerp(startRot, snapPoint.rotation, t);

            yield return null;
        }

        // Финальное положение
        transform.position = snapPoint.position;
        transform.rotation = snapPoint.rotation;

        // Устанавливаем родителя
        transform.SetParent(snapPoint, true);
        Debug.Log($"   Ключ прикреплен к {snapPoint.name}");

        // Дополнительная проверка через кадр
        StartCoroutine(FinalizeSnap());
    }

    private IEnumerator FinalizeSnap()
    {
        yield return null; // Ждем один кадр

        // Гарантируем, что kinematic включен
        if (rb != null && !rb.isKinematic)
        {
            rb.isKinematic = true;
            Debug.Log($"   Исправлено: Rigidbody.isKinematic = true");
        }

        Debug.Log($"✅ Ключ {keyType} успешно зафиксирован!");
    }

    public void ReleaseKey()
    {
        if (!IsSnapped) return;

        IsSnapped = false;
        Debug.Log($"🔓 Освобождаем ключ {keyType}...");

        // Возвращаем оригинального родителя
        if (originalParent != null)
            transform.SetParent(originalParent);
        else
            transform.SetParent(null);

        // Включаем физику
        if (rb != null)
        {
            rb.isKinematic = false;
            Debug.Log($"   Rigidbody: isKinematic = {rb.isKinematic}");
        }

        // Включаем XR взаимодействие
        if (xrInteractable != null)
        {
            xrInteractable.enabled = true;
            Debug.Log($"   Включен XR компонент: {xrInteractable.GetType().Name}");
        }

        // Включаем взаимодействие
        if (grabComponent != null)
        {
            grabComponent.enabled = true;
            Debug.Log($"   Включен {grabComponent.GetType().Name}");
        }

        // Включаем коллайдеры
        foreach (Collider col in colliders)
        {
            if (col != null && !col.enabled && !col.isTrigger)
            {
                col.enabled = true;
            }
        }
    }

    // Метод для проверки состояния
    [ContextMenu("Проверить состояние")]
    public void CheckState()
    {
        Debug.Log($"=== Состояние ключа {keyType} ===");
        Debug.Log($"IsSnapped: {IsSnapped}");
        Debug.Log($"Parent: {(transform.parent != null ? transform.parent.name : "None")}");
        Debug.Log($"Rigidbody: {(rb != null ? "Есть" : "Нет")}");
        Debug.Log($"isKinematic: {(rb != null ? rb.isKinematic.ToString() : "N/A")}");
        Debug.Log($"XR Interactable: {(xrInteractable != null ? xrInteractable.enabled.ToString() : "Нет")}");
        Debug.Log($"isSelected: {(xrInteractable != null ? xrInteractable.isSelected.ToString() : "N/A")}");
        Debug.Log($"GrabComponent: {(grabComponent != null ? grabComponent.enabled.ToString() : "Нет")}");
        Debug.Log($"=========================");
    }

    // Добавьте этот метод для принудительного освобождения ключа
    public void ForceReleaseFromXR()
    {
        if (xrInteractable != null && xrInteractable.isSelected)
        {
            Debug.Log($"🔄 Принудительное освобождение ключа {keyType} от XR");

            // Альтернативный способ: отключаем на 1 кадр
            StartCoroutine(TemporarilyDisableXR());
        }
    }

    private IEnumerator TemporarilyDisableXR()
    {
        if (xrInteractable != null)
        {
            bool wasEnabled = xrInteractable.enabled;
            xrInteractable.enabled = false;
            yield return null; // Ждем один кадр
            xrInteractable.enabled = wasEnabled;
        }
    }
}