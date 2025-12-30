using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(CharacterController))]
public class PlayerBodyController : MonoBehaviour
{
    [Header("绑定")]
    public Transform headCamera; // 拖入 Main Camera

    [Header("参数")]
    public LayerMask wallLayers; // 设为 Default
    public float gravity = -9.81f;
    public float bodyRadius = 0.2f;

    private CharacterController _cc;
    private Vector3 _verticalVelocity;
    private Vector3 _lastPos;

    void Start()
    {
        _cc = GetComponent<CharacterController>();
        _lastPos = transform.position;
    }

    void LateUpdate() // 使用 LateUpdate 确保在头动了之后再跟
    {
        if (headCamera == null) return;

        // 1. 【核心】手动同步胶囊体到头的位置
        // 既然关掉了 XRBodyTransformer，我们必须自己做这一步！
        Vector3 headLocalPos = transform.InverseTransformPoint(headCamera.position);
        _cc.center = new Vector3(headLocalPos.x, _cc.center.y, headLocalPos.z);

        // 2. 检测是否发生了传送 (瞬移)
        // 如果一帧内 Origin 移动超过 0.5米 (由上面的传送脚本触发)，这帧就不做物理检测
        if (Vector3.Distance(transform.position, _lastPos) > 0.5f)
        {
            _verticalVelocity = Vector3.zero;
            _lastPos = transform.position;
            return;
        }

        // 3. 应用重力
        ApplyGravity();

        // 4. 防穿墙推回
        // 因为我们上面已经把 center 同步到了头的位置，
        // 现在 _cc.bounds 就是你身体的真实位置
        CheckCollision();

        _lastPos = transform.position;
    }

    void ApplyGravity()
    {
        if (_cc.isGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = -2f; // 吸附地面
        }
        else
        {
            _verticalVelocity.y += gravity * Time.deltaTime;
        }
        _cc.Move(_verticalVelocity * Time.deltaTime);
    }

    void CheckCollision()
    {
        // 简单的防穿墙：如果胶囊体现在嵌在墙里，把它推出来
        // 获取胶囊体的上下底圆心
        Vector3 bottom = transform.TransformPoint(_cc.center + Vector3.down * (_cc.height / 2f - _cc.radius));
        Vector3 top = transform.TransformPoint(_cc.center + Vector3.up * (_cc.height / 2f - _cc.radius));

        // 稍微抬高一点 bottom 以免碰到地板
        bottom += Vector3.up * 0.2f;

        // 检测周围有没有墙
        Collider[] hits = Physics.OverlapCapsule(bottom, top, bodyRadius, wallLayers);

        foreach (var hit in hits)
        {
            // 如果撞墙，计算推离方向
            if (Physics.ComputePenetration(
                _cc, transform.position, transform.rotation,
                hit, hit.transform.position, hit.transform.rotation,
                out Vector3 dir, out float dist))
            {
                // 只在水平方向推，不把你推到天上去
                dir.y = 0;
                dir.Normalize();
                _cc.Move(dir * (dist + 0.01f));
            }
        }
    }
}

