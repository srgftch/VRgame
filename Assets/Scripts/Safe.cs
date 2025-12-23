using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Safe : MonoBehaviour
{
    [Header("Настройки сейфа")]
    public bool isLocked = true;
    public GameObject[] lockedObjects;    // Объекты которые скрыты/заблокированы
    public GameObject[] unlockedObjects;  // Объекты которые появляются/разблокируются

    [Header("Эффекты")]
    public AudioClip unlockSound;
    public ParticleSystem unlockParticles;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateSafeState();
    }

    public void Unlock()
    {
        if (!isLocked) return;

        isLocked = false;
        Debug.Log("Сейф открыт!");

        // Звук
        if (unlockSound != null)
            audioSource.PlayOneShot(unlockSound);

        // Эффекты
        if (unlockParticles != null)
            unlockParticles.Play();

        // Обновляем состояние объектов
        UpdateSafeState();
    }

    public void Lock()
    {
        if (isLocked) return;

        isLocked = true;
        UpdateSafeState();
    }

    private void UpdateSafeState()
    {
        // Скрываем/показываем объекты в зависимости от состояния
        foreach (GameObject obj in lockedObjects)
        {
            if (obj != null)
                obj.SetActive(isLocked);
        }

        foreach (GameObject obj in unlockedObjects)
        {
            if (obj != null)
                obj.SetActive(!isLocked);
        }
    }

    // УБИРАЕМ старый Input и делаем тестовые методы
    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.U))
    //     {
    //         Unlock();
    //     }
    //     
    //     if (Input.GetKeyDown(KeyCode.L))
    //     {
    //         Lock();
    //     }
    // }

    // Добавляем методы для тестирования через UI или другие способы
    public void TestUnlock()
    {
        Unlock();
    }

    public void TestLock()
    {
        Lock();
    }
}