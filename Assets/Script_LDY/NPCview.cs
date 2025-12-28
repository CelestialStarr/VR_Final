using UnityEngine;

public class NPCVision : MonoBehaviour
{
    public float detectRange = 10f;
    public LayerMask stealableLayer;
    public Transform eyesPoint;
    public float visionRadius = 0.3f;

    // 新增：报警UI（比如头顶冒个感叹号，或者屏幕中间出现“被通缉”）
    [Header("UI Feedback")]
    public GameObject alertUIPrefab; // 可选：报警时的特效/UI

    // 防止一直连续报警
    private bool hasAlertedPolice = false;

    void Update()
    {
        if (eyesPoint == null) return;

        // 如果正在报警冷却中（或者你希望每次看都报警，可以把这个逻辑去掉），可以加计时器重置 hasAlertedPolice
        CheckPlayerStealing();
    }

    private void CheckPlayerStealing()
    {
        Vector3 startPos = eyesPoint.position;
        Vector3 direction = eyesPoint.forward;
        RaycastHit hit;

        if (Physics.SphereCast(startPos, visionRadius, direction, out hit, detectRange, stealableLayer))
        {
            StealableObject target = hit.collider.GetComponentInParent<StealableObject>();

            if (target != null)
            {
                if (target.IsBeingStolen)
                {
                    // --- 核心修改区域 ---

                    Debug.LogError("NPC: 抓到你了！执行强制打断！");
                    target.HandleFailure(true);

                    // 1. 显示 NPC 发现你的 UI (可选)
                    if (alertUIPrefab != null && !hasAlertedPolice)
                    {
                        // 比如实例化一个感叹号
                        Instantiate(alertUIPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
                    }

                    // 2. 呼叫警察 (通过 Manager)
                    // 只有当没有刚刚报过警时才呼叫，防止每帧都Call
                    if (!hasAlertedPolice)
                    {
                        Debug.Log("NPC: 来人啊！抓小偷！");
                        // 通知管理器，传入当前 NPC 的位置作为案发现场
                        PoliceManager.Instance.DispatchNearestPolice(transform.position);

                        hasAlertedPolice = true;
                        // 建议：开启一个协程，5秒后重置 hasAlertedPolice = false，以便下次还能报警
                        Invoke(nameof(ResetAlert), 5f);
                    }

                    // -------------------
                }
                else
                {
                    // 看到了物体，但是安全的
                }
            }
        }
    }

    void ResetAlert()
    {
        hasAlertedPolice = false;
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