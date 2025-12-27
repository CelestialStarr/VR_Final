using UnityEngine;

public class UIFollowHead : MonoBehaviour
{
    public Transform head;
    public float distance = 0.5f;
    public float heightOffset = -0.15f; // UI 在视线下方
    public float smoothSpeed = 5f;

    void Update()
    {
        // 只取水平 forward（忽略抬头低头）
        Vector3 forward = head.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 targetPos =
            head.position +
            forward * distance +
            Vector3.up * heightOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * smoothSpeed
        );

        // 始终朝向头部（不仰不俯）
        Vector3 lookDir = transform.position - head.position;
        lookDir.y = 0;

        transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
