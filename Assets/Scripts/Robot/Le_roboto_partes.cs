using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Le_roboto_partes : MonoBehaviour
{
    [Header("Настройки части")]
    [SerializeField] private bool canAttach = true;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private Le_roboto_torso currentTorso;
    private Le_roboto_torso attachedTorso;
    private bool isGrabbed = false;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void Update()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && !isGrabbed && !IsAttachedToTorso())
        {
            // Если мы не захвачены и не прикреплены — ДОЛЖНЫ падать
            if (!rb.useGravity || rb.isKinematic)
            {
                rb.useGravity = true;
                rb.isKinematic = false;
                rb.WakeUp();
            }
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;

        if (attachedTorso != null)
        {
            attachedTorso.DetachPart(this);
            attachedTorso = null;
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;

        // Непосредственно при отпускании пытаемся присоединиться
        if (canAttach && currentTorso != null && attachedTorso == null)
        {
            currentTorso.AttachPart(this);
        }
    }

    public void OnAttachedToTorso(Le_roboto_torso torso)
    {
        if (!canAttach) return;
        attachedTorso = torso;
    }

    public void OnDetachedFromTorso()
    {
        attachedTorso = null;
    }

    public void SetCurrentTorso(Le_roboto_torso torso)
    {
        currentTorso = torso;
    }

    public void ClearCurrentTorso()
    {
        currentTorso = null;
    }

    public bool IsGrabbed()
    {
        return isGrabbed;
    }

    public bool IsAttachedToTorso()
    {
        return attachedTorso != null;
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}