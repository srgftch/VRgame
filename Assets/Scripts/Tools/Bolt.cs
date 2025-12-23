using UnityEngine;
using System;
using System.Collections;

public class Bolt : MonoBehaviour
{
    [Header("Bolt Settings")]
    public float unscrewDistance = 0.1f;
    public float rotationsToUnscrew = 5f;

    [Header("Physics Settings")]
    public bool enableGravityWhenUnscrewed = true;
    public bool becomePhysicalWhenUnscrewed = true;

    // Событие, которое вызывается при полном выкручивании болта
    public event Action OnBoltUnscrewed;

    private float currentRotation = 0f;
    private Vector3 initialPosition;
    private bool isScrewed = true;
    private Screwdriver currentScrewdriver;
    private Rigidbody rb;

    void Start()
    {
        initialPosition = transform.position;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void StartScrewing(Screwdriver screwdriver)
    {
        currentScrewdriver = screwdriver;
    }

    public void StopScrewing()
    {
        currentScrewdriver = null;
    }

    public void RotateBolt(float angle)
    {
        if (!isScrewed) return;

        float direction = Mathf.Sign(angle);
        currentRotation += Mathf.Abs(angle);

        transform.Rotate(0, angle, 0, Space.World);

        float progress = Mathf.Clamp01(currentRotation / (rotationsToUnscrew * 360f));
        transform.position = initialPosition + transform.up * (progress * unscrewDistance);

        if (progress >= 1f)
        {
            isScrewed = false;
            OnBoltUnscrewedInternal();
        }
    }

    private void OnBoltUnscrewedInternal()
    {
        Debug.Log("Болт выкручен!");

        // Запускаем плавное перемещение
        StartCoroutine(MoveUpwards());

        // Вызываем событие
        OnBoltUnscrewed?.Invoke();

        if (currentScrewdriver != null)
        {
            currentScrewdriver.DisengageFromBolt();
        }
    }

    private IEnumerator MoveUpwards()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + Vector3.up * 100f;
        float duration = 1.0f; // Длительность анимации в секундах
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        // Включаем физику после перемещения
        if (rb != null && becomePhysicalWhenUnscrewed)
        {
            rb.isKinematic = false;
            rb.useGravity = enableGravityWhenUnscrewed;
            rb.AddForce(transform.up * 0.5f, ForceMode.Impulse);
        }
    }

    public void UnscrewInstantly()
    {
        if (!isScrewed) return;

        Debug.Log("Мгновенное откручивание болта!");

        // Устанавливаем максимальное вращение для полного откручивания
        currentRotation = rotationsToUnscrew * 360f;

        // Обновляем позицию болта (выдвигаем его)
        float progress = Mathf.Clamp01(currentRotation / (rotationsToUnscrew * 360f));
        transform.position = initialPosition + transform.up * (progress * unscrewDistance);

        // Отмечаем как открученный и вызываем события
        isScrewed = false;

        // ВЫЗЫВАЕМ СОБЫТИЕ для уведомления решетки
        OnBoltUnscrewed?.Invoke();

        // Вызываем логику завершения откручивания
        CompleteUnscrewing();
    }

    private void CompleteUnscrewing()
    {
        Debug.Log("Болт полностью выкручен!");

        // Телепортируем болт на 100 единиц вверх
        transform.position += Vector3.up * 100f;

        // Включаем физику для болта
        if (rb != null && becomePhysicalWhenUnscrewed)
        {
            rb.isKinematic = false;
            rb.useGravity = enableGravityWhenUnscrewed;
            rb.AddForce(transform.up * 0.5f, ForceMode.Impulse);
        }

        if (currentScrewdriver != null)
        {
            currentScrewdriver.DisengageFromBolt();
        }
    }

    // Метод для сброса состояния болта
    public void ResetBolt()
    {
        currentRotation = 0f;
        isScrewed = true;
        transform.position = initialPosition;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Свойство для проверки состояния болта
    public bool IsScrewed
    {
        get { return isScrewed; }
    }
}