using UnityEngine;

public class HandGestureModeController : MonoBehaviour
{
    public enum HandMode
    {
        None,
        Point,
        Palm
    }

    [Header("Mode Objects")]
    public GameObject teleportObject;   // Teleport Ray / Teleport Interactor
    public GameObject menuObject;       // Menu Root

    [Header("Current State (Debug)")]
    [SerializeField]
    private HandMode currentMode = HandMode.None;

    //  Point Gesture Performed
    public void OnPointPerformed()
    {
        // 如果已经在 Palm 模式，Point 无效
        if (currentMode == HandMode.Palm)
            return;

        currentMode = HandMode.Point;

        teleportObject.SetActive(true);
        menuObject.SetActive(false);

        Debug.Log("[Gesture] Point → Teleport ON");
    }

    //  Palm Gesture Performed
    public void OnPalmPerformed()
    {
        // 如果已经在 Point 模式，Palm 无效
        if (currentMode == HandMode.Point)
            return;

        currentMode = HandMode.Palm;

        menuObject.SetActive(true);
        teleportObject.SetActive(false);

        Debug.Log("[Gesture] Palm → Menu ON");
    }

    //  Fist Gesture Performed（统一关闭）
    public void OnFistPerformed()
    {
        teleportObject.SetActive(false);
        menuObject.SetActive(false);

        currentMode = HandMode.None;

        Debug.Log("[Gesture] Fist → Reset to None");
    }

    public bool IsInMode(HandMode mode)
    {
        return currentMode == mode;
    }

    public HandMode GetCurrentMode()
    {
        return currentMode;
    }
}
