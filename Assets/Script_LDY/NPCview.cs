using UnityEngine;

public class NPCVision : MonoBehaviour
{

    [Header("Layers")]
    public LayerMask stealableLayer;
    public LayerMask playerLayer;

    public Transform eyesPoint;
    public float visionRadius = 0.3f;

    [Header("Alert Settings")]
    public AlertController alertController; // 【新增】引用外部脚本

    [Header("Vision Settings")]
    public float detectRange = 10f;
    public float alertIncreaseAmount = 30f; // 【新增】每次增加多少警戒值
    public float alertInterval = 0.5f;      // 【新增】每隔多久增加一次（比如0.5秒）
    private float currentAlertTimer = 0f;   // 内部计时器

    [SerializeField] private bool isInPrivateArea = false;

    // 新增：报警UI（比如头顶冒个感叹号，或者屏幕中间出现“被通缉”）
    [Header("UI Feedback")]
    public GameObject alertUIPrefab; // 可选：报警时的特效/UI

    // 防止一直连续报警
    private bool hasAlertedPolice = false;

    private void Start()
    {
        if (alertController == null)
            alertController = GetComponent<AlertController>();
    }

    void Update()
    {
        if (eyesPoint == null) return;

        // 如果正在报警冷却中（或者你希望每次看都报警，可以把这个逻辑去掉），可以加计时器重置 hasAlertedPolice
        CheckVision();
    }
    // 【新增】检测 NPC 是否进入了 PrivateArea
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PrivateArea"))
        {
            isInPrivateArea = true;
            // 可选：在这里重置一下计时器
            currentAlertTimer = 0f;
        }
    }
    // 【新增】检测 NPC 是否离开了 PrivateArea
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("PrivateArea"))
        {
            isInPrivateArea = false;
        }
    }

    private void CheckVision()
    {
        Vector3 startPos = eyesPoint.position;
        Vector3 direction = eyesPoint.forward;
        RaycastHit hit;

        // 【修改关键点】
        // 我们需要同时检测“可偷窃物品”和“玩家”。
        // 所以将两个 LayerMask 进行“或”运算 (|) 组合在一起检测。
        LayerMask combinedMask = stealableLayer | playerLayer;

        if (Physics.SphereCast(startPos, visionRadius, direction, out hit, detectRange, combinedMask))
        {
            GameObject hitObj = hit.collider.gameObject;

            // =================================================
            // 情况 A：检测是否在私人区域看到玩家 (Private Area Logic)
            // =================================================
            if (isInPrivateArea && hitObj.CompareTag("Player"))
            {
                Debug.Log("See Player");
                // 确保引用了 AlertController
                if (alertController != null)
                {
                    // 只有当没有进入“完全战斗状态(isAlerted)”时，才慢慢增加条
                    // (如果已经 Alerted 了，通常直接开打或报警，不需要再增加条了)
                    if (!alertController.isAlerted)
                    {
                        currentAlertTimer += Time.deltaTime;

                        if (currentAlertTimer >= alertInterval)
                        {
                            // 调用 AlertController 增加警戒值
                            alertController.IncreaseAlert(alertIncreaseAmount);
                            Debug.Log("私人区域发现玩家！警戒值增加！");

                            // 重置计时器
                            currentAlertTimer = 0f;
                        }
                    }
                }
                return; // 如果看到了玩家，优先处理玩家逻辑，不再处理物品
            }

            // =================================================
            // 情况 B：检测偷窃 (Stealing Logic - 原有逻辑)
            // =================================================
            // 尝试获取 StealableObject 组件 (防止射线打到玩家身上报错，加个判空)
            StealableObject target = hitObj.GetComponentInParent<StealableObject>();

            if (target != null)
            {
                if (target.IsBeingStolen)
                {
                    Debug.LogError("NPC: 抓到你了！执行强制打断！");
                    target.HandleFailure(true);

                    // 1. UI 反馈
                    if (alertUIPrefab != null && !hasAlertedPolice)
                    {
                        Instantiate(alertUIPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
                    }

                    // 2. 呼叫警察
                    if (!hasAlertedPolice)
                    {
                        Debug.Log("NPC: 来人啊！抓小偷！");
                        // 假设 PoliceManager 存在
                        if (PoliceManager.Instance != null)
                            PoliceManager.Instance.DispatchNearestPolice(transform.position);

                        hasAlertedPolice = true;

                        // 这里也应该瞬间拉满警戒值（逻辑上抓现行了）
                        if (alertController != null)
                            alertController.IncreaseAlert(alertController.maxAlertValue);

                        Invoke(nameof(ResetAlert), 5f);
                    }
                }
            }
        }
        else
        {
            // 如果视线丢失了玩家，重置一下计时器，这样下次看到会有个微小的延迟，比较真实
            currentAlertTimer = 0f;
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