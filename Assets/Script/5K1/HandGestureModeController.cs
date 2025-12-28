using UnityEngine;

public class HandGestureModeController : MonoBehaviour
{
    public enum HandMode
    {
        None,
        Palm
    }

    [Header("Controlled Objects")]
    public GameObject teleportObject;
    public GameObject menuObject;

    [Header("Two-Hand Point Settings")]
    public float simultaneousThreshold = 0.25f; // 同时判定时间窗口（秒）

    [Header("Debug")]
    [SerializeField] private HandMode currentMode = HandMode.None;

    private float leftPointTime = -1f;
    private float rightPointTime = -1f;

    //  Left Hand Point Performed
    public void OnLeftHandPoint()
    {
        leftPointTime = Time.time;
        EvaluateTeleport();
    }

    //  Right Hand Point Performed
    public void OnRightHandPoint()
    {
        rightPointTime = Time.time;
        EvaluateTeleport();
    }

    //  Fist（统一关闭）
    public void OnFist()
    {
        ResetAll();
        Debug.Log("[Gesture] Fist → Reset");
    }

    //  Palm（打开菜单，强制退出 teleport）
    public void OnPalm()
    {
        currentMode = HandMode.Palm;

        teleportObject.SetActive(false);
        menuObject.SetActive(true);

        ResetPointTimes();

        Debug.Log("[Gesture] Palm → Menu ON");
    }

    private void EvaluateTeleport()
    {
        // Palm 模式下禁止 teleport
        if (currentMode == HandMode.Palm)
            return;

        // 两只手是否在时间窗口内同时 Point
        if (Mathf.Abs(leftPointTime - rightPointTime) <= simultaneousThreshold)
        {
            teleportObject.SetActive(true);
            Debug.Log("Two Hands SIMULTANEOUS Point → Teleport ON");
        }
        else
        {
            teleportObject.SetActive(false);
        }
    }

    private void ResetAll()
    {
        ResetPointTimes();
        teleportObject.SetActive(false);
        menuObject.SetActive(false);
        currentMode = HandMode.None;
    }

    private void ResetPointTimes()
    {
        leftPointTime = -1f;
        rightPointTime = -1f;
    }
}
