using UnityEngine;

public class SimpleWallCollision : MonoBehaviour
{
    [Header("组件设置")]
    public Transform headCamera; // 拖入 Main Camera
    public CharacterController characterController;

    [Header("参数微调")]
    // 胶囊体高度
    public float fixedHeight = 1.7f;

    // --- 新增：重力相关设置 ---
    [Header("重力设置")]
    [Tooltip("重力加速度，建议设大一点(如-20)让下落更干脆")]
    public float gravity = -20.0f;

    [Tooltip("在斜坡上行走时的吸地力，数值越大贴得越紧")]
    public float stickToGroundForce = -10.0f;

    // 用于存储当前的垂直速度
    private Vector3 _velocity;

    private void Start()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        // 关键设置：把最小移动距离设为0
        characterController.minMoveDistance = 0f;
    }

    private void FixedUpdate()
    {
        if (headCamera == null || characterController == null) return;

        // --- 1. 让胶囊体(身体)跟随相机(头) (保持不变) ---
        Vector3 cameraLocalPos = transform.InverseTransformPoint(headCamera.position);

        // 这里的Y轴保持胶囊体高度的一半，也可以根据需求改成动态蹲下逻辑
        Vector3 targetCenter = new Vector3(cameraLocalPos.x, characterController.height / 2, cameraLocalPos.z);

        characterController.center = targetCenter;
        characterController.height = fixedHeight;

        // --- 2. 物理“微颤” (防穿墙的核心，保持不变) ---
        // 这两句是为了强制触发物理碰撞检测，防止头部穿墙
        characterController.Move(new Vector3(0.0001f, -0.0001f, 0.0001f));
        characterController.Move(new Vector3(-0.0001f, 0.0001f, -0.0001f));

        // --- 3. 新增：重力吸附逻辑 (解决下坡飘的问题) ---
        ApplyGravity();
    }

    // 处理重力的方法
    private void ApplyGravity()
    {
        // isGrounded 是 CharacterController 自带的状态，判断底部是否接触地面
        if (characterController.isGrounded)
        {
            // 如果在地面上，且速度是向下的，将其重置为一个固定的吸地力
            // 这就是让你在下坡时紧紧“吸”住地面的关键
            if (_velocity.y < 0)
            {
                _velocity.y = stickToGroundForce;
            }
        }
        else
        {
            // 如果悬空了（比如走下台阶），累加重力加速度
            _velocity.y += gravity * Time.fixedDeltaTime;
        }

        // 应用垂直方向的移动
        // 注意：Move 方法会自动处理碰撞，不会让你穿过地板
        characterController.Move(_velocity * Time.fixedDeltaTime);
    }
}