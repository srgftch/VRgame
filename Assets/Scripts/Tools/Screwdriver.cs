using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Screwdriver : MonoBehaviour
{
    [Header("Screwdriver Settings")]
    public float rotationThreshold = 15f;

    private XRGrabInteractable grabInteractable;
    private Bolt currentBolt;
    private Quaternion lastRotation;
    private bool isEngaged = false;

    void Start()
    {
        // Получаем ссылку на XRGrabInteractable
        grabInteractable = GetComponent<XRGrabInteractable>();

        // Подписываемся на события
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void Update()
    {
        if (isEngaged && currentBolt != null)
        {
            ProcessRotation();
        }
    }

    private void ProcessRotation()
    {
        Quaternion currentRot = transform.rotation;
        float angleDelta = Quaternion.Angle(lastRotation, currentRot);

        if (angleDelta > rotationThreshold)
        {
            Vector3 oldForward = lastRotation * Vector3.forward;
            Vector3 newForward = currentRot * Vector3.forward;
            Vector3 cross = Vector3.Cross(oldForward, newForward);
            float direction = Vector3.Dot(cross, transform.up) > 0 ? 1 : -1;

            currentBolt.RotateBolt(angleDelta * direction);
            lastRotation = currentRot;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!grabInteractable.isSelected || isEngaged) return;

        Bolt bolt = other.GetComponent<Bolt>();
        if (bolt != null)
        {
            currentBolt = bolt;
            isEngaged = true;
            lastRotation = transform.rotation;
            bolt.StartScrewing(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Bolt bolt = other.GetComponent<Bolt>();
        if (bolt == currentBolt && isEngaged)
        {
            Disengage();
        }
    }

    // Этот метод должен быть публичным, чтобы Bolt мог его вызвать
    public void DisengageFromBolt()
    {
        Disengage();
    }

    private void Disengage()
    {
        if (currentBolt != null)
        {
            currentBolt.StopScrewing();
            currentBolt = null;
        }
        isEngaged = false;
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Дополнительные действия при захвате
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        Disengage();
    }

    void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
    }
}