using UnityEngine;

public class UIHeadFollow : MonoBehaviour
{
    [Header("跟随设置")]
    public float distance = 1.0f;       // UI 距离眼睛多远 (米)
    public float smoothTime = 0.3f;     // 跟随的平滑度 (数值越大越慢/越平滑)

    [Header("高度修正")]
    public bool lockYAxis = false;      // 如果勾选，UI就不会随着你抬头低头而移动，只会在水平方向跟随
    public float heightOffset = 0.0f;   // 手动调整垂直高度

    private Transform cameraTransform;
    private Vector3 currentVelocity;

    private void Start()
    {
        // 自动找到 VR 摄像机 (Main Camera)
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("找不到 Main Camera！请确保你的 XR Origin 里的摄像机有 MainCamera 的 Tag。");
        }
    }

    private void LateUpdate() // 使用 LateUpdate 保证在摄像机移动后才计算 UI 位置，防止抖动
    {
        if (cameraTransform == null) return;

        // 1. 计算目标位置：在摄像机正前方 distance 米处
        Vector3 targetPosition = cameraTransform.position + (cameraTransform.forward * distance);

        // 如果想锁定 Y 轴 (不想让 UI 随抬头低头乱跑)，就固定 Y 坐标
        if (lockYAxis)
        {
            targetPosition.y = cameraTransform.position.y + heightOffset;
        }
        else
        {
            targetPosition.y += heightOffset;
        }

        // 2. 位置平滑插值 (关键：让 UI 像果冻一样飘过去，而不是瞬移)
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        // 3. 旋转：让 UI 始终正对着玩家的脸
        // 使用 LookAt 让 UI 面板朝向摄像机
        // 注意：UI 默认是背对着 forward 的，所以我们要让它看向摄像机相反的方向，或者直接让它的旋转等于摄像机的旋转
        // 这里用 Slerp 做平滑旋转
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - cameraTransform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / smoothTime);
    }
}