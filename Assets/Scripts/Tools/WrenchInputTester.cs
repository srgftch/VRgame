using UnityEngine;

public class WrenchInputTester : MonoBehaviour
{
    [SerializeField] public SelfControlledWrench wrench;

    void Update()
    {
        //// Тестируем все возможные кнопки
        //string[] testButtons = {
        //    "XRI_Right_Trigger",
        //    "XRI_Left_Trigger",
        //    "Fire1",
        //    "Fire2",
        //    "Submit",
        //    "Jump",
        //    "Space" // для проверки в Editor
        //};

        //foreach (string button in testButtons)
        //{
        //    if (Input.GetButtonDown(button))
        //    {
        //        Debug.Log($"Кнопка нажата: {button}");
        //        if (wrench != null)
        //        {
        //            wrench.TestStartRotation();
        //        }
        //    }

        //    if (Input.GetButtonUp(button))
        //    {
        //        Debug.Log($"Кнопка отпущена: {button}");
        //        if (wrench != null)
        //        {
        //            wrench.TestStopRotation();
        //        }
        //    }
        //}

        //// Тест пробелом
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Debug.Log("Пробел нажат");
        //    if (wrench != null)
        //    {
        //        wrench.TestStartRotation();
        //    }
        //}

        //if (Input.GetKeyUp(KeyCode.Space))
        //{
        //    Debug.Log("Пробел отпущен");
        //    if (wrench != null)
        //    {
        //        wrench.TestStopRotation();
        //    }
        //}
    }
}