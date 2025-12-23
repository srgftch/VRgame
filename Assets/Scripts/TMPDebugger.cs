using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanvasDebugger : MonoBehaviour
{
    public Canvas canvas;
    public TMP_Text scoreText;

    void Start()
    {
        if (canvas == null)
            canvas = GetComponent<Canvas>();

        if (scoreText == null)
            scoreText = GetComponentInChildren<TMP_Text>();

        Debug.Log("=== Canvas Debugger ===");
        Debug.Log($"Canvas: {canvas?.name ?? "null"}");
        Debug.Log($"Canvas enabled: {canvas?.enabled}");
        Debug.Log($"Canvas render mode: {canvas?.renderMode}");
        Debug.Log($"Canvas camera: {canvas?.worldCamera?.name ?? "none"}");
        Debug.Log($"Canvas sorting layer: {canvas?.sortingLayerName}");
        Debug.Log($"Canvas order in layer: {canvas?.sortingOrder}");

        if (scoreText != null)
        {
            Debug.Log($"TMP Text: {scoreText.gameObject.name}");
            Debug.Log($"TMP enabled: {scoreText.enabled}");
            Debug.Log($"TMP gameObject active: {scoreText.gameObject.activeSelf}");
            Debug.Log($"TMP text: '{scoreText.text}'");
            Debug.Log($"TMP color: {scoreText.color}");
            Debug.Log($"TMP alpha: {scoreText.color.a}");
            Debug.Log($"TMP font size: {scoreText.fontSize}");
            Debug.Log($"TMP rect position: {scoreText.rectTransform.anchoredPosition}");
            Debug.Log($"TMP rect size: {scoreText.rectTransform.sizeDelta}");
            Debug.Log($"TMP parent: {scoreText.transform.parent?.name}");

            // Визуальная проверка - временно меняем цвет
            scoreText.color = Color.red;
        }

        // Принудительно обновляем Canvas
        Canvas.ForceUpdateCanvases();
    }

    void Update()
    {
        // Тестовая кнопка для проверки
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (scoreText != null)
            {
                scoreText.text = $"Тест: {Time.time:F2}";
                Debug.Log($"Текст изменен на: '{scoreText.text}'");
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 400, 20), "Нажмите T для теста текста");

        if (scoreText != null)
        {
            GUI.Label(new Rect(10, 30, 400, 40),
                     $"TMP активен: {scoreText.gameObject.activeInHierarchy}");
            GUI.Label(new Rect(10, 50, 400, 40),
                     $"Текст: '{scoreText.text}'");
            GUI.Label(new Rect(10, 70, 400, 40),
                     $"Позиция: {scoreText.rectTransform.position}");
            GUI.Label(new Rect(10, 90, 400, 40),
                     $"Альфа цвета: {scoreText.color.a}");
        }
    }
}