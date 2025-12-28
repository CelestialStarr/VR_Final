using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleLock : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("判定成功的角度范围 (例如 15 代表 +/- 15度)")]
    [SerializeField] private float rangeTolerance = 15f;

    [Tooltip("解锁所需的基准时间（秒）")]
    [SerializeField] private float requiredTime = 3.0f;

    [Tooltip("当完全对准中心时，时间流逝的倍率 (越准越快)")]
    [SerializeField] private float maxSpeedMultiplier = 5.0f;

    [Header("Debug Info")]
    [SerializeField] private float targetAngle; // 生成的随机目标值
    [SerializeField] private float currentUnlockTimer; // 当前剩余时间
    [SerializeField] private bool isUnlocked = false;

    private AudioSource audioSource;

    [Header("绑定门控制器")]
    [Tooltip("请把挂有 LockedDoor 脚本的门物体拖到这里")]
    public LockedDoor targetDoor;

    private bool hasUnlocked = false;


    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // 确保音效是循环的，这样只要在范围内就会一直响
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    // 当物体被设置为 Active 时调用
    void OnEnable()
    {
        ResetLock();
    }

    void OnDisable()
    {
        // 物体关闭时强制停止音效
        if (audioSource.isPlaying) audioSource.Stop();
    }

    void Update()
    {
        if (isUnlocked) return; // 如果已经解锁，不再执行逻辑

        // 1. 获取当前物体的 Z 轴旋转，并规范化到 -180 ~ 180 度
        float currentAngle = GetXAngleFromVector();

        // 2. 计算当前角度与目标角度的差值 (Mathf.DeltaAngle 处理了跨越 180/-180 的情况)
        float angleDifference = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

        // 3. 判断是否在 [x-15, x+15] 范围内
        if (angleDifference <= rangeTolerance)
        {
            HandleAudio(true);
            ProcessUnlockLogic(angleDifference);
        }
        else
        {
            HandleAudio(false);
            // 离开范围是否要重置时间？通常这种游戏机制离开范围会暂停或缓慢回退。
            // 这里为了符合"离开停止"的简单描述，我们暂不做重置，只是不减少时间。
        }
    }
    // 【核心方法】利用向量计算真实的 X 轴角度 (-180 ~ 180)
    private float GetXAngleFromVector()
    {
        // 1. 获取物体当前的“上方”指向哪里
        //    (transform.localRotation * Vector3.up) 代表物体自身坐标系的绿色箭头指向
        Vector3 currentUp = transform.localRotation * Vector3.up;

        // 2. 计算这个指向和标准“上方”(Vector3.up) 的夹角
        //    第三个参数 Vector3.right (X轴) 用来确定方向（正数还是负数）
        //    如果你的物体是向左转，它会根据 X 轴判断出是负数
        float angle = Vector3.SignedAngle(Vector3.up, currentUp, Vector3.right);

        return angle;
    }
    // 重置锁的状态
    private void ResetLock()
    {
        // 生成 [-165, 165] 的随机值
        targetAngle = Random.Range(-165f, 165f);
        currentUnlockTimer = requiredTime;
        isUnlocked = false;

        Debug.Log($"Lock Reset! Target Angle: {targetAngle}");
    }

    // 处理音效播放状态
    private void HandleAudio(bool shouldPlay)
    {
        if (shouldPlay)
        {
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying) audioSource.Stop();
        }
    }

    // 处理倒计时逻辑
    private void ProcessUnlockLogic(float diff)
    {
        // 计算速度倍率：
        // 位于边缘 (diff = 15) 时，倍率为 1
        // 位于中心 (diff = 0) 时，倍率为 maxSpeedMultiplier (例如 5倍速)
        // 使用 Lerp 根据距离插值
        float lerpFactor = 1f - (diff / rangeTolerance); // 0 (边缘) -> 1 (中心)
        float currentSpeed = Mathf.Lerp(1f, maxSpeedMultiplier, lerpFactor);

        // 减少剩余时间
        currentUnlockTimer -= Time.deltaTime * currentSpeed;

        if (currentUnlockTimer <= 0)
        {
            UnlockSuccess();
        }
    }

    private void UnlockSuccess()
    {
        isUnlocked = true;
        currentUnlockTimer = 0;
        audioSource.Stop();
        Debug.Log("Unlock Successful!");
        hasUnlocked = true;

        // --- 核心修改：调用 LockedDoor 的功能 ---
        if (targetDoor != null)
        {
            // 调用我们在上一个脚本里写的公共方法
            targetDoor.UnlockTheDoor();
        }
        else
        {
            Debug.LogError("请在 Inspector 面板中给 LockSystem 脚本赋值 Target Door！");
        }
        // 在这里添加解锁后的逻辑，例如调用其他脚本或播放成功音效
    }

    // 辅助函数：将 Unity 的 0~360 角度转换为 -180~180
    private float NormalizeAngle(float angle)
    {
        angle %= 360;
        if (angle > 180) return angle - 360;
        return angle;
    }

    // 可视化辅助：在 Scene 窗口画出目标范围，方便调试
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        // 绘制目标角度线
        Vector3 direction = Quaternion.Euler(0, 0, targetAngle) * Vector3.up;
        Gizmos.DrawRay(transform.position, direction * 2);

        // 绘制扇形区域 (大概示意)
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        // 注意：Gizmos 绘制比较简略，仅供参考位置
    }
}