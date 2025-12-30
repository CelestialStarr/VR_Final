using UnityEngine;

public class HandForwardMoveByGesture : MonoBehaviour
{
    [Header("References")]
    public Transform handTransform;      // Hand Anchor
    public Transform cameraTransform;    // XR Camera

    [Header("Move Settings")]
    public float moveSpeed = 0.3f;
    public bool lockY = true;

    private bool isMoving = false;

    void Update()
    {
        if (!isMoving) return;

        MoveHandForward();
    }

    void MoveHandForward()
    {
        Vector3 forward = cameraTransform.forward;

        if (lockY)
            forward.y = 0;

        handTransform.position += forward.normalized * moveSpeed * Time.deltaTime;
    }

    // =========================
    // 🟢 给手势系统调用
    // =========================

    /// <summary>
    /// 手势识别成功时调用
    /// </summary>
    public void StartMove()
    {
        isMoving = true;
    }

    /// <summary>
    /// 手势松开 / 失败 / 中断时调用
    /// </summary>
    public void StopMove()
    {
        isMoving = false;
    }
}
