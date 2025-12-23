using UnityEngine;
using System.Collections;

public class SimpleDoorLock : MonoBehaviour
{
    [System.Serializable]
    public class Socket
    {
        public Key.KeyType socketType;
        public Collider triggerCollider;
        public Transform snapPoint;
        public Renderer visualFeedback;

        [HideInInspector] public bool isLocked = false;
        [HideInInspector] public Key currentKey;
    }

    [Header("Гнезда")]
    public Socket[] sockets;

    [Header("Дверь")]
    public Transform doorPivot;
    public float openAngle = -90f;
    public float openSpeed = 1f;

    [Header("Материалы")]
    public Material idleMat;
    public Material activeMat;
    public Material lockedMat;

    [Header("Настройки ключей")]
    public float keyDestroyDelay = 0.5f; // Задержка перед уничтожением ключей

    private bool isDoorOpen = false;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Debug.Log($"🚪 SimpleDoorLock инициализирован. Сокетов: {sockets?.Length ?? 0}");

        if (sockets != null)
        {
            foreach (Socket socket in sockets)
            {
                if (socket != null && socket.triggerCollider != null)
                {
                    Debug.Log($"  ✅ Сокет {socket.socketType}: {socket.triggerCollider.name}");

                    TriggerHandler handler = socket.triggerCollider.gameObject.AddComponent<TriggerHandler>();
                    handler.parentSocket = socket;
                    handler.doorLock = this;

                    if (socket.visualFeedback != null && idleMat != null)
                        socket.visualFeedback.material = idleMat;
                }
                else
                {
                    Debug.LogError($"  ❌ Сокет {socket?.socketType}: нет триггера!");
                }
            }
        }
    }

    public void OnKeyEntered(Socket socket, Key key)
    {
        Debug.Log($"🔑 Ключ {key.keyType} вошел в сокет {socket.socketType}");

        if (socket.isLocked) return;

        if (key.keyType == socket.socketType)
        {
            socket.currentKey = key;
            StartCoroutine(SnapKey(socket, key));
        }
        else
        {
            Debug.Log($"❌ Неправильный ключ! Ожидался {socket.socketType}, получили {key.keyType}");
        }
    }

    IEnumerator SnapKey(Socket socket, Key key)
    {
        Debug.Log($"🔄 Начинаем фиксацию ключа {key.keyType}...");

        if (socket.visualFeedback != null && activeMat != null)
            socket.visualFeedback.material = activeMat;

        yield return new WaitForSeconds(0.1f);

        key.SnapToPosition(socket.snapPoint);
        socket.isLocked = true;

        if (socket.visualFeedback != null && lockedMat != null)
            socket.visualFeedback.material = lockedMat;

        Debug.Log($"✅ Ключ {key.keyType} зафиксирован!");

        CheckAllSockets();
    }

    void CheckAllSockets()
    {
        if (sockets == null) return;

        bool allLocked = true;
        foreach (Socket socket in sockets)
        {
            if (socket != null && !socket.isLocked)
            {
                allLocked = false;
                break;
            }
        }

        if (allLocked && !isDoorOpen)
        {
            Debug.Log("🎉 ВСЕ КЛЮЧИ ВСТАВЛЕНЫ! Открываем дверь...");
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        isDoorOpen = true;
        StartCoroutine(OpenDoorAnimation());

        // Уничтожаем ключи после открытия двери
        StartCoroutine(DestroyKeysAfterDelay());
    }

    IEnumerator OpenDoorAnimation()
    {
        if (doorPivot == null) yield break;

        Quaternion startRot = doorPivot.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, openAngle, 0);
        float elapsed = 0;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * openSpeed;
            doorPivot.localRotation = Quaternion.Slerp(startRot, endRot, elapsed);
            yield return null;
        }

        Debug.Log("🚪 Дверь открыта!");
    }

    // Новый метод: уничтожение ключей с задержкой
    IEnumerator DestroyKeysAfterDelay()
    {
        // Ждем указанное время перед уничтожением
        yield return new WaitForSeconds(keyDestroyDelay);

        Debug.Log($"🗑️ Уничтожаем ключи...");

        if (sockets != null)
        {
            foreach (Socket socket in sockets)
            {
                if (socket != null && socket.currentKey != null)
                {
                    Debug.Log($"   Уничтожаем ключ типа {socket.currentKey.keyType}");
                    Destroy(socket.currentKey.gameObject);
                    socket.currentKey = null;
                }
            }
        }
    }

    // Альтернативный метод: скрытие ключей вместо уничтожения
    public void HideKeys()
    {
        Debug.Log($"👻 Скрываем ключи...");

        if (sockets != null)
        {
            foreach (Socket socket in sockets)
            {
                if (socket != null && socket.currentKey != null)
                {
                    Debug.Log($"   Скрываем ключ типа {socket.currentKey.keyType}");
                    socket.currentKey.gameObject.SetActive(false);
                }
            }
        }
    }

    [ContextMenu("Сбросить все")]
    public void ResetAll()
    {
        StopAllCoroutines();
        isDoorOpen = false;

        if (doorPivot != null)
            doorPivot.localRotation = Quaternion.identity;

        if (sockets != null)
        {
            foreach (Socket socket in sockets)
            {
                if (socket != null)
                {
                    socket.isLocked = false;
                    // Не уничтожаем ключи при сбросе, просто обнуляем ссылку
                    socket.currentKey = null;

                    if (socket.visualFeedback != null && idleMat != null)
                        socket.visualFeedback.material = idleMat;
                }
            }
        }

        Debug.Log("🔄 Все сброшено");
    }
}

public class TriggerHandler : MonoBehaviour
{
    public SimpleDoorLock.Socket parentSocket;
    public SimpleDoorLock doorLock;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🎯 {name}: {other.name} вошел", other.gameObject);

        if (parentSocket == null || doorLock == null) return;

        Key key = other.GetComponent<Key>();
        if (key != null)
        {
            doorLock.OnKeyEntered(parentSocket, key);
        }
    }
}