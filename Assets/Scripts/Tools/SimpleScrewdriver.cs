using UnityEngine;

public class SimpleScrewdriver : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Bolt bolt = other.GetComponent<Bolt>();
        if (bolt != null && bolt.IsScrewed)
        {
            // Мгновенно откручиваем болт
            bolt.UnscrewInstantly();
            Debug.Log("Болт мгновенно откручен!");
        }
    }
}