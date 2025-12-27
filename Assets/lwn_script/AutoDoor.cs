using UnityEngine;

public class RotatingAutoDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public Transform doorPivot;      // 门的旋转轴心（建议用空物体包裹门模型）
    public float openAngle = 90f;    // 开启的角度
    public float rotateSpeed = 150f; // 旋转速度（度/秒）

    private Quaternion closedRot;
    private Quaternion openRot;
    private bool isOpen = false;

    void Start()
    {
        if (doorPivot == null) doorPivot = transform;

        // 记录初始旋转（关门状态）
        closedRot = doorPivot.localRotation;

        // 计算开门旋转（假设绕 Y 轴旋转）
        // 如果方向反了，把 openAngle 改成 -openAngle
        openRot = closedRot * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        // 目标旋转取决于 isOpen 状态
        Quaternion targetRot = isOpen ? openRot : closedRot;

        // 使用 RotateTowards 稳定旋转，避免飞走或报错
        doorPivot.localRotation = Quaternion.RotateTowards(
            doorPivot.localRotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOpen = false;
        }
    }
}