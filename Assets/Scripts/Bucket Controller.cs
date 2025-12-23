using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Settings")]
    public TMP_Text scoreTextTMP;
    public string scorePrefix = "Мышей: ";

    private int totalMiceCaught = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // НЕТ DontDestroyOnLoad - сбрасывается при загрузке новой сцены
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateScoreUI();
    }

    public void AddMouseCaught(int amount = 1)
    {
        totalMiceCaught += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreTextTMP != null)
        {
            scoreTextTMP.text = scorePrefix + totalMiceCaught.ToString();
        }
    }

    public int GetScore()
    {
        return totalMiceCaught;
    }

    public void ResetScore()
    {
        totalMiceCaught = 0;
        UpdateScoreUI();
    }
}