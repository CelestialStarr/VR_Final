using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils; // 【关键】引用 XROrigin

// 这个脚本不依赖 LocomotionMediator 的具体类型，直接操作 XROrigin
public class UniversalTeleport : TeleportationProvider
{
    protected override void Update()
    {
        // 1. 如果没有传送请求，直接退出
        if (!validRequest) return;

        // 2. 直接找场景里的 XR Origin
        // 我们不通过 System 或 Mediator 找了，直接找核心组件，这样最稳
        XROrigin xrOrigin = FindObjectOfType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogError("【严重错误】场景里找不到 XROrigin！请检查是否挂载了 XROrigin 脚本。");
            return;
        }

        // 3. 执行传送逻辑
        Vector3 targetPos = currentRequest.destinationPosition;

        // A. 计算偏移 (头显和脚底的水平距离)
        Vector3 camPos = xrOrigin.Camera.transform.position;
        Vector3 originPos = xrOrigin.Origin.transform.position;
        Vector3 camOffset = camPos - originPos;
        camOffset.y = 0; // 忽略高度

        // B. 计算 Origin 应该去的新位置
        Vector3 finalOriginPos = targetPos - camOffset;
        finalOriginPos.y = targetPos.y; // 保持在目标点高度

        // C. 瞬间移动
        xrOrigin.Origin.transform.position = finalOriginPos;

        // D. 处理旋转 (如果需要)
        if (currentRequest.matchOrientation == MatchOrientation.TargetUpAndForward)
        {
            float currentRotY = xrOrigin.Camera.transform.eulerAngles.y;
            float targetRotY = currentRequest.destinationRotation.eulerAngles.y;
            float angleDiff = targetRotY - currentRotY;

            xrOrigin.Origin.transform.Rotate(0, angleDiff, 0);
        }

        // 4. 结束传送状态
        validRequest = false;
    }
}