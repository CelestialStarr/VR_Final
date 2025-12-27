using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SmartWallBlocker : MonoBehaviour
{
    [Header("组件设置")]
    public Transform headCamera;

    [Header("参数设置")]
    public float bodyRadius = 0.2f;
    public LayerMask wallLayers; // 记得设置这个！！

    [Header("重力设置")]
    public float gravity = -20.0f;
    public float stickToGroundForce = -10.0f;

    private CharacterController _cc;
    private Vector3 _velocity;

    // 用来记录上一帧头部在虚拟世界的位置
    private Vector3 _lastHeadPosition;

    private void Start()
    {
        _cc = GetComponent<CharacterController>();
        _cc.minMoveDistance = 0f;
        _cc.skinWidth = 0.05f; // 皮肤宽度设小一点，防止卡墙
        _lastHeadPosition = headCamera.position;
    }

    private void FixedUpdate()
    {
        if (headCamera == null) return;

        // 1. 高度同步
        float targetHeight = Mathf.Clamp(headCamera.localPosition.y, 0.5f, 2.5f);
        _cc.height = targetHeight;

        // 2. 身体跟随
        Vector3 headLocalPos = transform.InverseTransformPoint(headCamera.position);
        Vector3 targetCenter = new Vector3(headLocalPos.x, _cc.height / 2f, headLocalPos.z);
        _cc.center = targetCenter;

        // 3. 视觉阻挡逻辑 (抬腿版)
        Vector3 desiredMove = headCamera.position - _lastHeadPosition;
        desiredMove.y = 0;

        if (desiredMove.magnitude > 0.001f)
        {
            // --- 核心修改开始 ---

            // 原来的写法：从胶囊体中心检测（会撞到脚下的台阶）
            // Vector3 capsulePos = transform.TransformPoint(_cc.center);

            // 现在的写法：把检测起点抬高！
            // 抬高的高度 = 台阶高度限制 + 一点点缓冲
            // 这样 0.3 米以下的障碍物，射线根本检测不到，就不会触发“推回”，你就能跨上去了
            float checkHeightOffset = _cc.stepOffset + 0.05f;

            // 我们基于脚底位置来算
            Vector3 feetPos = transform.position;
            // 抬高起点
            Vector3 rayStartPos = feetPos + Vector3.up * checkHeightOffset;

            // 注意：SphereCast 只要发射一点点距离，且只检测“膝盖”以上
            if (Physics.SphereCast(rayStartPos, bodyRadius, desiredMove.normalized, out RaycastHit hit, desiredMove.magnitude + 0.05f, wallLayers))
            {
                // 既然我们已经抬高了检测线，只要撞到了，那肯定就是高墙，直接推回
                Vector3 pushBack = -desiredMove;
                _cc.Move(pushBack);
            }
            // --- 核心修改结束 ---
        }

        _lastHeadPosition = headCamera.position;

        // 4. 重力
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
        // 这里只是应用重力，水平移动由上面的 PushBack 控制
        _cc.Move(_velocity * Time.fixedDeltaTime);
    }
}