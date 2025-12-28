using UnityEngine;

public class NPCVision : MonoBehaviour
{
    public float detectRange = 10f;
    public LayerMask stealableLayer;

    [Header("EyePoint")]
    public Transform eyesPoint;

    // 新增：视线粗细 (防止射线太细导致检测不稳定)
    public float visionRadius = 0.3f;

    void Update()
    {
        if (eyesPoint == null) return;
        CheckPlayerStealing();
    }

    private void CheckPlayerStealing()
    {
        Vector3 startPos = eyesPoint.position;
        Vector3 direction = eyesPoint.forward;
        RaycastHit hit;

        // 【修改 1】使用 SphereCast (球形投射) 代替 Raycast
        // 就像发射一根粗水管，比激光笔更容易打中物体
        if (Physics.SphereCast(startPos, visionRadius, direction, out hit, detectRange, stealableLayer))
        {
            // 调试线：红色表示物理检测到了 (Hit)
            Debug.DrawLine(startPos, hit.point, Color.red);

            // 【修改 2】使用 GetComponentInParent
            // 防止碰撞体在子物体上，而脚本在父物体上导致找不到
            StealableObject target = hit.collider.GetComponentInParent<StealableObject>();

            if (target != null)
            {
                // 检查状态
                if (target.IsBeingStolen)
                {
                    Debug.Log("NPC: 抓到你了！执行强制打断！"); // 用 LogError 醒目一点
                    target.HandleFailure(true); // 传入 true 强制松手
                }
                else
                {
                    // 只有这行 Log 出现，说明你没在偷，或者 _isStealing 状态卡在 false 了
                    Debug.Log("NPC: 看到了物体，但它现在的状态是【安全】。");
                }
            }
            else
            {
                // 如果这行出现，说明 Layer 对了，但脚本没挂对位置
              //  Debug.LogWarning($"NPC: 射线打到了 {hit.collider.name}，但找不到 StealableObject 脚本！");
            }
        }
        else
        {
            // 调试线：绿色表示没打中任何 Layer 为 Stealable 的东西
            Debug.DrawRay(startPos, direction * detectRange, Color.green);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (eyesPoint != null)
        {
            Gizmos.color = Color.yellow;
            // 画出球形射线的范围
            Gizmos.DrawWireSphere(eyesPoint.position, visionRadius);
            Gizmos.DrawRay(eyesPoint.position, eyesPoint.forward * detectRange);
        }
    }
}