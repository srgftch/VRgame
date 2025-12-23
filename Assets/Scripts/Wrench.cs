using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Wrench : MonoBehaviour
{
    [Header("Соединение")]
    public Transform socketPoint; // ← назначить в инспекторе!

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (!grabInteractable)
        {
            Debug.LogError("Wrench: нужен XRGrabInteractable!");
            return;
        }

        if (rb == null)
        {
            Debug.LogError("Wrench: нужен Rigidbody!");
            return;
        }

        // НЕ трогаем isKinematic и gravity в Start!
        // XR Grab Interactable сам управляет этим при захвате/отпускании.

        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    public void StartUnscrewSequence(NewBolt bolt)
    {
        // Отключаем взаимодействие на мгновение
        grabInteractable.enabled = false;

        // Позиционируем
        transform.position = bolt.transform.position;
        transform.rotation = bolt.transform.rotation;

        // Запускаем откручивание
        bolt.Unscrew();

        // Через задержку — включаем обратно и даём упасть
        Invoke(nameof(EnableInteractionAndDrop), 0.2f);
    }

    void EnableInteractionAndDrop()
    {
        // Включаем обратно, чтобы можно было поднять
        grabInteractable.enabled = true;

        // Убеждаемся, что физика включена (она и так должна быть)
        rb.useGravity = true;
        // НЕ трогаем isKinematic — XR сам решает!
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        // Ничего не делаем с Rigidbody!
        // XR Grab Interactable автоматически:
        // - делает тело кинематическим при захвате
        // - сбрасывает velocity/angularVelocity
        // - отключает гравитацию
    }

    void OnReleased(SelectExitEventArgs args)
    {
        // Ничего не делаем — XR сам включит гравитацию и физику
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}