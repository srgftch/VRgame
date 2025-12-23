using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class XRPartFix : MonoBehaviour
{
    [Header("XR Настройки")]
    public bool disableXRWhenAttached = true;

    private XRGrabInteractable xrGrabInteractable;
    private Rigidbody rb;
    private bool wasXREnabled = true;

    void Start()
    {
        xrGrabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
    }

    // Вызывается когда часть прикрепляется к торсу
    public void OnXRPartAttached()
    {
        if (disableXRWhenAttached && xrGrabInteractable != null)
        {
            wasXREnabled = xrGrabInteractable.enabled;
            xrGrabInteractable.enabled = false;

            // Также отключаем все XR компоненты
            XRBaseInteractable[] interactables = GetComponents<XRBaseInteractable>();
            foreach (var interactable in interactables)
            {
                interactable.enabled = false;
            }

            Debug.Log($"XR отключен для {name}");
        }

        // Для kinematic объектов важно сбросить velocity
        if (rb != null && rb.isKinematic)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // Вызывается когда часть отсоединяется от торса
    public void OnXRPartDetached()
    {
        if (disableXRWhenAttached && xrGrabInteractable != null)
        {
            xrGrabInteractable.enabled = wasXREnabled;

            // Включаем все XR компоненты
            XRBaseInteractable[] interactables = GetComponents<XRBaseInteractable>();
            foreach (var interactable in interactables)
            {
                interactable.enabled = true;
            }

            Debug.Log($"XR включен для {name}");
        }
    }
}