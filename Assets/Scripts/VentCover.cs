using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VentCover : MonoBehaviour
{
    [Header("Vent Cover Settings")]
    public Bolt[] bolts;
    public int requiredUnscrewedBolts = 1;

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private bool isActive = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Изначально решетка статична
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Отключаем возможность взаимодействия
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        Debug.Log($"Решетка инициализирована. Болтов: {bolts.Length}, нужно открутить: {requiredUnscrewedBolts}");
    }

    void Update()
    {
        // Постоянно проверяем состояние болтов
        if (!isActive)
        {
            CheckBoltsStatus();
        }
    }

    private void CheckBoltsStatus()
    {
        int unscrewedCount = 0;

        foreach (Bolt bolt in bolts)
        {
            if (bolt != null && !bolt.IsScrewed)
            {
                unscrewedCount++;
            }
        }

        // Если достаточно болтов откручено - активируем решетку
        if (unscrewedCount >= requiredUnscrewedBolts)
        {
            ActivateVentCover();
        }

        // Для отладки (можно убрать после тестирования)
        if (unscrewedCount > 0)
        {
            Debug.Log($"Откручено болтов: {unscrewedCount}/{requiredUnscrewedBolts}");
        }
    }

    private void ActivateVentCover()
    {
        isActive = true;
        Debug.Log("Решетка активирована! Теперь можно двигать.");

        // Включаем физику
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        // Включаем возможность взаимодействия
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }
    }
}