using UnityEngine;

public class NPCVision : MonoBehaviour
{
    public float detectRange = 10f;
    public LayerMask stealableLayer;
    [Header("EyePoint")]
    public Transform eyesPoint;

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

        // 发射射线
        if (Physics.Raycast(startPos, direction, out hit, detectRange, stealableLayer))
        {
            // 调试线：红色表示看中物体了
            Debug.DrawLine(startPos, hit.point, Color.red);

            // 获取物体脚本
            StealableObject target = hit.collider.GetComponent<StealableObject>();

            // 如果物体存在，并且处于“正在被偷”的状态
            if (target != null && target.IsBeingStolen)
            {
                Debug.Log("NPC: 抓到你了！交出来！");

                // 【核心代码】直接调用物体的失败逻辑，强制它脱手并归位
                target.HandleFailure();
            }
        }
        else
        {
            // 调试线：绿色表示安全
            Debug.DrawRay(startPos, direction * detectRange, Color.green);
        }
    }

    // Gizmos 画线辅助
    private void OnDrawGizmosSelected()
    {
        if (eyesPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(eyesPoint.position, eyesPoint.forward * detectRange);
        }
    }
}