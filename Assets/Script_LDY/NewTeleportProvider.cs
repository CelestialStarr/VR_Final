using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

// 继承官方的 TeleportationProvider，保留所有 Inspector 里的参数和功能
public class CustomTeleportationProvider : TeleportationProvider
{
    // 用于处理延迟计时
    private bool _isDelaying = false;
    private float _delayTimer = 0f;

    // 重写 Update，复刻官方的流程，但替换核心移动逻辑
    protected override void Update()
    {
        // 1. 如果没有收到传送请求，什么都不做 (和官方一样)
        if (!validRequest)
        {
            return;
        }

        // 2. 处理延迟时间 (Delay Time) - 完美复刻官方逻辑
        if (delayTime > 0)
        {
            // 如果还没开始延迟，初始化计时器
            if (!_isDelaying)
            {
                _isDelaying = true;
                _delayTimer = delayTime;

                // 可以在这里触发 "开始传送" 的事件，如果你需要的话
                return;
            }

            // 倒计时
            _delayTimer -= Time.deltaTime;
            if (_delayTimer > 0)
            {
                return; // 还在冷却，等待
            }
        }

        // --- 3. 核心修改区域：执行传送 ---
        // 官方代码在这里是调用 QueueTransformation 交给 BodyTransformer
        // 我们改为：直接计算坐标并移动

        PerformDirectTeleport();

        // 4. 收尾工作 (和官方一样)
        validRequest = false;     // 标记请求已处理
        _isDelaying = false;      // 重置延迟状态
    }

    private void PerformDirectTeleport()
    {
        var xrOrigin = system.xrOrigin;
        if (xrOrigin != null)
        {
            Vector3 targetPos = currentRequest.destinationPosition;

            // --- 核心：手动计算移动，绕过 BodyTransformer ---

            // 1. 获取摄像机相对于 Origin 的偏移 (只取水平面 X,Z)
            Vector3 camPos = xrOrigin.Camera.transform.position;
            Vector3 originPos = xrOrigin.Origin.transform.position;
            Vector3 camOffset = camPos - originPos;
            camOffset.y = 0; // 忽略高度差

            // 2. 计算 Origin 应该去的新位置
            // 新 Origin = 目标点 - 摄像机偏移
            // 这样你的头 (Camera) 就会准确落在目标点上
            Vector3 newOriginPos = targetPos - camOffset;

            // 3. 高度处理：直接覆盖为目标点高度
            newOriginPos.y = targetPos.y;

            // 4. 执行瞬间移动！
            // 因为这是一帧内的巨大位移，你的防穿墙脚本会判定为传送并放行
            xrOrigin.Origin.transform.position = newOriginPos;

            // 5. 处理旋转 (如果有方向要求)
            if (currentRequest.matchOrientation == MatchOrientation.TargetUpAndForward)
            {
                // 获取目标朝向
                Vector3 targetForward = currentRequest.destinationRotation * Vector3.forward;
                targetForward.y = 0;
                targetForward.Normalize();

                // 获取当前摄像机朝向
                Vector3 cameraForward = xrOrigin.Camera.transform.forward;
                cameraForward.y = 0;
                cameraForward.Normalize();

                // 计算角度差并旋转 Origin
                float angleDiff = Vector3.SignedAngle(cameraForward, targetForward, Vector3.up);
                xrOrigin.Origin.transform.Rotate(0, angleDiff, 0);
            }
        }
    }
}