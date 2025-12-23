using UnityEngine;
using System.Collections;

public class NewBolt : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("Поведение")]
    public RotationAxis rotationAxis = RotationAxis.Z; // ← выбери в инспекторе!
    public float unscrewAngle = 90f;        // угол откручивания
    public float unscrewDuration = 0.3f;    // длительность анимации

    [Header("Визуал")]
    public Renderer boltRenderer;
    public Material idleMat;
    public Material unscrewedMat;

    private bool isUnscrewed = false;

    public bool IsScrewed => !isUnscrewed;

    void Start()
    {
        UpdateVisuals();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isUnscrewed) return;

        Wrench wrench = other.GetComponentInParent<Wrench>();
        if (wrench != null)
        {
            wrench.StartUnscrewSequence(this);
        }
    }

    public void Unscrew()
    {
        if (isUnscrewed) return;
        isUnscrewed = true;

        StartCoroutine(UnscrewAnimation());
    }

    IEnumerator UnscrewAnimation()
    {
        Vector3 eulerOffset = Vector3.zero;
        switch (rotationAxis)
        {
            case RotationAxis.X: eulerOffset = new Vector3(-unscrewAngle, 0, 0); break;
            case RotationAxis.Y: eulerOffset = new Vector3(0, -unscrewAngle, 0); break;
            case RotationAxis.Z: eulerOffset = new Vector3(0, 0, -unscrewAngle); break;
        }

        Quaternion startRot = transform.localRotation;
        Quaternion targetRot = startRot * Quaternion.Euler(eulerOffset);

        float elapsed = 0f;
        while (elapsed < unscrewDuration)
        {
            elapsed += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, elapsed / unscrewDuration);
            yield return null;
        }

        transform.localRotation = targetRot;
        UpdateVisuals();
        Debug.Log($"✅ Болт {name} откручен по оси {rotationAxis}!");
    }

    void UpdateVisuals()
    {
        if (boltRenderer != null)
        {
            boltRenderer.material = isUnscrewed ? unscrewedMat : idleMat;
        }
    }

    [ContextMenu("Сбросить")]
    public void ResetBolt()
    {
        isUnscrewed = false;
        transform.localRotation = Quaternion.identity;
        UpdateVisuals();
    }
}