// Assets/Scripts/VictoryScreen.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    [Header("Ссылки")]
    public GameObject victoryPanel; // Canvas → Panel с надписью "Победа!"

    void OnEnable()
    {
        Le_roboto_torso.OnRobotAssembled += ShowVictory;
    }

    void OnDisable()
    {
        Le_roboto_torso.OnRobotAssembled -= ShowVictory;
    }

    void ShowVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; // Опционально: заморозить игру
        }
        else
        {
            Debug.LogWarning("VictoryPanel не назначен!");
        }
    }

    // Кнопка "Назад в меню"
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // ← замени на имя своей сцены
    }

    // Кнопка "Перезапустить"
    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}