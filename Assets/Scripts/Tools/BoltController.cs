using UnityEngine;
using UnityEngine.Events;

public class BoltController : MonoBehaviour
{
    //[Header("Bolt Settings")]
    //public float unscrewAngle = 720f;
    //public float currentUnscrewAngle = 0f;
    //public bool isUnscrewed = false;
    //public Transform wrenchSocket;

    //[Header("Visual Settings")]
    //public GameObject boltModel;
    //public float unscrewHeight = 0.1f;
    //private Vector3 originalPosition;

    //[Header("Effects")]
    //public ParticleSystem unscrewEffect;
    //public AudioClip unscrewSound;
    //public AudioClip completeSound;

    //[Header("Events")]
    //public UnityEvent onBoltStartUnscrewing;
    //public UnityEvent onBoltUnscrewed;
    //public UnityEvent onBoltRemoved;

    //private SelfControlledWrench attachedWrench; // ИЗМЕНИЛ ТИП
    //private AudioSource audioSource;
    //private bool isUnscrewing = false;

    //void Start()
    //{
    //    audioSource = GetComponent<AudioSource>();
    //    if (audioSource == null)
    //    {
    //        audioSource = gameObject.AddComponent<AudioSource>();
    //    }

    //    if (wrenchSocket == null)
    //    {
    //        GameObject socket = new GameObject("WrenchSocket");
    //        socket.transform.SetParent(transform);
    //        socket.transform.localPosition = Vector3.zero;
    //        socket.transform.localRotation = Quaternion.identity;
    //        wrenchSocket = socket.transform;
    //    }

    //    if (boltModel != null)
    //    {
    //        originalPosition = boltModel.transform.localPosition;
    //    }
    //    else
    //    {
    //        originalPosition = transform.localPosition;
    //    }
    //}

    //void Update()
    //{
    //    if (isUnscrewing && !isUnscrewed)
    //    {
    //        UpdateBoltVisual();
    //    }
       

    //}

    //public void OnWrenchAttached(SelfControlledWrench wrench) // ИЗМЕНИЛ ПАРАМЕТР
    //{
    //    attachedWrench = wrench;
    //    Debug.Log($"Болт {name} получил ключ");
    //}

    //public void OnWrenchDetached()
    //{
    //    attachedWrench = null;
    //    Debug.Log($"Ключ откреплен от болта {name}");
    //}

    //public void StartUnscrewing()
    //{
    //    if (!isUnscrewed && attachedWrench != null)
    //    {
    //        isUnscrewing = true;
    //        onBoltStartUnscrewing.Invoke();

    //        if (unscrewSound != null && audioSource != null)
    //        {
    //            audioSource.clip = unscrewSound;
    //            audioSource.loop = true;
    //            audioSource.Play();
    //        }

    //        if (unscrewEffect != null)
    //        {
    //            unscrewEffect.Play();
    //        }
    //    }
    //}

    //public void StopUnscrewing()
    //{
    //    isUnscrewing = false;

    //    if (audioSource != null && audioSource.isPlaying)
    //    {
    //        audioSource.Stop();
    //    }

    //    if (unscrewEffect != null)
    //    {
    //        unscrewEffect.Stop();
    //    }
    //}

    //public void UpdateUnscrewProgress(float angleDelta)
    //{
    //    if (!isUnscrewed)
    //    {
    //        currentUnscrewAngle += angleDelta;

    //        if (currentUnscrewAngle >= unscrewAngle)
    //        {
    //            CompleteUnscrewing();
    //        }
    //    }
    //}

    //void UpdateBoltVisual()
    //{
    //    if (boltModel != null)
    //    {
    //        float progress = Mathf.Clamp01(currentUnscrewAngle / unscrewAngle);
    //        Vector3 newPosition = originalPosition + Vector3.up * (unscrewHeight * progress);
    //        boltModel.transform.localPosition = newPosition;
    //        boltModel.transform.localRotation = Quaternion.Euler(0, currentUnscrewAngle, 0);
    //    }
    //}

    //void CompleteUnscrewing()
    //{
    //    isUnscrewed = true;
    //    isUnscrewing = false;

    //    if (audioSource != null && audioSource.isPlaying)
    //    {
    //        audioSource.Stop();
    //    }

    //    if (completeSound != null && audioSource != null)
    //    {
    //        audioSource.PlayOneShot(completeSound);
    //    }

    //    if (attachedWrench != null)
    //    {
    //        attachedWrench.DetachFromBolt();
    //    }

    //    onBoltUnscrewed.Invoke();
    //    Debug.Log($"Болт {name} полностью откручен!");
    //}

    //void OnDrawGizmosSelected()
    //{
    //    if (wrenchSocket != null)
    //    {
    //        Gizmos.color = Color.cyan;
    //        Gizmos.DrawWireSphere(wrenchSocket.position, 0.02f);
    //        Gizmos.DrawLine(wrenchSocket.position, wrenchSocket.position + wrenchSocket.up * 0.05f);
    //    }

    //    if (!isUnscrewed)
    //    {
    //        float progress = currentUnscrewAngle / unscrewAngle;
    //        Gizmos.color = Color.Lerp(Color.red, Color.green, progress);
    //        Gizmos.DrawWireSphere(transform.position, 0.05f * (1 + progress));
    //    }
    //    else
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawWireSphere(transform.position, 0.06f);
    //    }
    //}
}