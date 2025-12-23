using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider))]
public class ElectricalPanelDoor : MonoBehaviour
{
    [Header("Болты")]
    public NewBolt[] bolts; // ← используем твой NewBolt (из предыдущего кода)

    [Header("Дверца")]
    public Transform doorPivot; // петля — точка вращения
    public float openAngle = -90f; // угол открытия (в градусах вокруг Y)
    public float openDuration = 1.0f; // длительность анимации открытия

    [Header("Визуал / Аудио")]
    public Renderer panelRenderer;
    public Material lockedMaterial;
    public Material unlockedMaterial;
    public AudioSource audioSource;
    public AudioClip unlockSound;
    public AudioClip openSound;

    private bool isUnlocked = false;
    private bool isDoorOpen = false;

    void Start()
    {
        if (bolts == null || bolts.Length == 0)
        {
            Debug.LogError("ElectricalPanelDoor: не назначены болты!");
            return;
        }

        UpdateVisuals();
        Debug.Log($"🔌 Электрощиток инициализирован. Болтов: {bolts.Length}");
    }

    void Update()
    {
        if (!isUnlocked)
        {
            CheckBoltsStatus();
        }
    }

    void CheckBoltsStatus()
    {
        bool allUnscrewed = true;
        foreach (NewBolt bolt in bolts)
        {
            if (bolt == null || bolt.IsScrewed)
            {
                allUnscrewed = false;
                break;
            }
        }

        if (allUnscrewed && !isUnlocked)
        {
            UnlockPanel();
        }
    }

    void UnlockPanel()
    {
        isUnlocked = true;
        UpdateVisuals();

        if (audioSource != null && unlockSound != null)
            audioSource.PlayOneShot(unlockSound);

        Debug.Log("🔓 Все болты откручены! Щиток разблокирован.");

        // Можно сразу открыть, или дать пользователю самому потянуть — выберем автоматическое открытие как у сейфа
        StartCoroutine(OpenDoorCoroutine());
    }

    IEnumerator OpenDoorCoroutine()
    {
        if (isDoorOpen || doorPivot == null) yield break;

        isDoorOpen = true;

        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);

        Quaternion startRot = doorPivot.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0f, openAngle, 0f);
        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / openDuration;
            doorPivot.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        doorPivot.localRotation = targetRot;
        Debug.Log("🚪 Дверца электрощитка открыта!");
    }

    void UpdateVisuals()
    {
        if (panelRenderer == null) return;

        panelRenderer.material = isUnlocked ? unlockedMaterial : lockedMaterial;
    }

    [ContextMenu("Сбросить щиток")]
    public void ResetPanel()
    {
        StopAllCoroutines();
        isUnlocked = false;
        isDoorOpen = false;

        if (doorPivot != null)
            doorPivot.localRotation = Quaternion.identity;

        if (bolts != null)
        {
            foreach (NewBolt bolt in bolts)
            {
                if (bolt != null)
                {
                    bolt.ResetBolt(); // используем твой метод из NewBolt
                }
            }
        }

        UpdateVisuals();
        Debug.Log("🔄 Щиток сброшен");
    }
}