using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Le_roboto_torso : MonoBehaviour
{
    [Header("Настройки присоединения")]
    [SerializeField] private string attachableTag = "RobotoPart";
    [SerializeField] private Transform attachPoint;
    [SerializeField] private bool snapToPoint = true;
    [SerializeField] private Vector3 attachOffset = Vector3.zero;
    [SerializeField] private Vector3 attachRotation = Vector3.zero;

    [Header("Триггер зона")]
    [SerializeField] private Collider attachmentTrigger;
    [SerializeField] private float checkInterval = 0.1f;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private List<Le_roboto_partes> partsInTrigger = new List<Le_roboto_partes>();
    private Dictionary<Le_roboto_partes, Transform> attachedParts = new Dictionary<Le_roboto_partes, Transform>();
    private Dictionary<Le_roboto_partes, bool> originalGravityStates = new Dictionary<Le_roboto_partes, bool>();
    private float lastCheckTime;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (attachmentTrigger != null)
        {
            attachmentTrigger.isTrigger = true;
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void Update()
    {
        if (Time.time - lastCheckTime < checkInterval) return;
        lastCheckTime = Time.time;

        // Проверяем части в триггере для присоединения
        for (int i = partsInTrigger.Count - 1; i >= 0; i--)
        {
            var part = partsInTrigger[i];
            if (part == null)
            {
                partsInTrigger.RemoveAt(i);
                continue;
            }

            if (!part.IsGrabbed() && !attachedParts.ContainsKey(part))
            {
                AttachPart(part);
            }
        }

        // Проверяем присоединенные части для отсоединения
        var keys = new List<Le_roboto_partes>(attachedParts.Keys);
        foreach (var part in keys)
        {
            if (part == null)
            {
                attachedParts.Remove(part);
                originalGravityStates.Remove(part);
                continue;
            }

            if (part.IsGrabbed() || !partsInTrigger.Contains(part))
            {
                DetachPart(part);
            }
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args) { }

    private void OnReleased(SelectExitEventArgs args)
    {
        foreach (var part in partsInTrigger)
        {
            if (part != null && !part.IsGrabbed() && !attachedParts.ContainsKey(part))
            {
                AttachPart(part);
            }
        }
    }

    public void AttachPart(Le_roboto_partes part)
    {
        if (part == null || attachedParts.ContainsKey(part)) return;

        // Сохраняем оригинального родителя
        Transform originalParent = part.transform.parent;
        attachedParts[part] = originalParent;

        // Получаем Rigidbody и сохраняем оригинальное состояние гравитации
        Rigidbody partRb = part.GetComponent<Rigidbody>();
        if (partRb != null)
        {
            originalGravityStates[part] = partRb.useGravity;

            // ЗАМОРАЖИВАЕМ ФИЗИКУ И ВЫКЛЮЧАЕМ ГРАВИТАЦИЮ
            partRb.constraints = RigidbodyConstraints.FreezeAll;
            partRb.useGravity = false;
            partRb.velocity = Vector3.zero;
            partRb.angularVelocity = Vector3.zero;
            partRb.isKinematic = false; // Убедимся, что не kinematic
        }

        // Устанавливаем родителя и позицию
        if (snapToPoint && attachPoint != null)
        {
            part.transform.SetParent(attachPoint);
            part.transform.localPosition = attachOffset;
            part.transform.localEulerAngles = attachRotation;
        }
        else
        {
            part.transform.SetParent(transform);
        }

        // Уведомляем часть о присоединении
        part.OnAttachedToTorso(this);
    }

    public void DetachPart(Le_roboto_partes part)
    {
        if (part == null || !attachedParts.ContainsKey(part)) return;

        Rigidbody partRb = part.GetComponent<Rigidbody>();
        if (partRb != null)
        {
            // 1. Отсоединяем от родителя (в корень сцены)
            part.transform.SetParent(null);

            // 2. Фиксируем физику
            partRb.useGravity = true;
            partRb.isKinematic = false;
            partRb.constraints = RigidbodyConstraints.None;

            // 3. Обнуляем, но даём лёгкий "толчок вниз"
            partRb.velocity = Vector3.down * 0.01f; // ← важно!
            partRb.angularVelocity = Vector3.zero;

            // 4. Пробуждаем
            partRb.WakeUp();

            // Лог для отладки:
            Debug.Log($"✅ {part.name} отсоединён: grav={partRb.useGravity}, kin={partRb.isKinematic}, vel={partRb.velocity}", partRb);
        }

        part.OnDetachedFromTorso();
        attachedParts.Remove(part);
        originalGravityStates.Remove(part);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(attachableTag))
        {
            Le_roboto_partes part = other.GetComponent<Le_roboto_partes>();
            if (part != null && !partsInTrigger.Contains(part))
            {
                partsInTrigger.Add(part);
                part.SetCurrentTorso(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(attachableTag))
        {
            Le_roboto_partes part = other.GetComponent<Le_roboto_partes>();
            if (part != null)
            {
                partsInTrigger.Remove(part);

                if (attachedParts.ContainsKey(part))
                {
                    DetachPart(part);
                }

                part.ClearCurrentTorso();
            }
        }
    }

    public bool IsPartAttached(Le_roboto_partes part)
    {
        return attachedParts.ContainsKey(part);
    }

    public void DetachAllParts()
    {
        var keys = new List<Le_roboto_partes>(attachedParts.Keys);
        foreach (var part in keys)
        {
            DetachPart(part);
        }
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }

        DetachAllParts();
    }
}