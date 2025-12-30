using UnityEngine;

// 强制要求有 CharacterController 组件
[RequireComponent(typeof(CharacterController))]
public class SmartLocomotionController : MonoBehaviour
{
    [Header("核心设置")]
    public Transform headCamera; // 必须拖入 Main Camera
    public LayerMask wallLayers; // 必须设置为 Default 或墙壁层

    [Header("参数微调")]
    public float bodyRadius = 0.2f; // 身体半径
    public float stepHeight = 0.35f; // 台阶/跨步高度 (低于这个高度不算墙)

    [Header("传送兼容 (重点)")]
    [Tooltip("如果一帧内水平移动超过这个距离，视为传送，不做物理阻挡。")]
    public float teleportThreshold = 0.5f;

    [Header("重力设置")]
    public float gravity = -20.0f;
    public float stickToGroundForce = -10.0f;

    // 内部变量
    private CharacterController _cc;
    private Vector3 _lastHeadPos;
    private Vector3 _velocity;
    private bool _isFirstFrame = true;

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
        // 确保 CharacterController 参数正确，否则容易卡顿
        _cc.stepOffset = 0.3f;
        _cc.minMoveDistance = 0f;
        _cc.skinWidth = 0.05f;
    }

    private void FixedUpdate()
    {
        if (headCamera == null) return;

        // 1. 初始化位置记录
        if (_isFirstFrame)
        {
            _lastHeadPos = headCamera.position;
            _isFirstFrame = false;
            return;
        }

        // --- 第一步：同步高度和中心点 (替代 XR Body Transformer 的功能) ---
        // 实时调整胶囊体高度，支持下蹲
        float targetHeight = Mathf.Clamp(headCamera.localPosition.y, 0.5f, 2.5f);
        _cc.height = targetHeight;

        // 实时调整中心点，确保胶囊体跟着头走
        Vector3 headLocalPos = transform.InverseTransformPoint(headCamera.position);
        Vector3 targetCenter = new Vector3(headLocalPos.x, _cc.height / 2f, headLocalPos.z);
        _cc.center = targetCenter;

        // --- 第二步：智能防穿墙与传送判断 ---

        // 计算这一帧头移动了多少
        Vector3 currentHeadPos = headCamera.position;
        Vector3 moveVector = currentHeadPos - _lastHeadPos;

        // 只看水平移动 (忽略上下楼梯的Y轴变化)
        Vector3 horizontalMove = moveVector;
        horizontalMove.y = 0;
        float moveDist = horizontalMove.magnitude;

        // 【关键逻辑】：判断是“走路”还是“传送”
        if (moveDist > teleportThreshold)
        {
            // === 情况A：传送 ===
            // 移动距离巨大（>0.5米），认为是传送。
            // 直接接受新位置，不进行任何阻挡检测。
            _velocity = Vector3.zero; // 重置重力速度
            _lastHeadPos = currentHeadPos; // 更新记录点
            // Debug.Log("检测到传送，脚本放行！"); 
            return;
        }

        // === 情况B：正常走路 (或者撞墙) ===
        if (moveDist > 0.001f)
        {
            // 抬高检测起点，忽略脚下的台阶 (膝盖高度检测法)
            Vector3 feetPos = transform.position;
            Vector3 rayStart = feetPos + Vector3.up * stepHeight; // 从 0.35m 高度发射射线
            float checkDist = moveDist + 0.1f; // 多检测一点点

            // 发射射线检测前方是否有墙
            if (Physics.SphereCast(rayStart, bodyRadius, horizontalMove.normalized, out RaycastHit hit, checkDist, wallLayers))
            {
                // 撞墙了！把身体反向推回去
                // Debug.Log("撞墙阻挡：" + hit.collider.name);
                Vector3 pushBack = -horizontalMove;
                _cc.Move(pushBack);
            }
            else
            {
                // 没撞墙，正常更新记录点
                _lastHeadPos = currentHeadPos;
            }
        }
        else
        {
            // 没动
            _lastHeadPos = currentHeadPos;
        }

        // --- 第三步：重力吸附 (解决下坡飘浮) ---
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (_cc.isGrounded)
        {
            if (_velocity.y < 0) _velocity.y = stickToGroundForce;
        }
        else
        {
            _velocity.y += gravity * Time.fixedDeltaTime;
        }
        _cc.Move(_velocity * Time.fixedDeltaTime);
    }
}