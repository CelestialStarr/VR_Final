using UnityEngine;

public class NPCVision : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask stealableLayer;
    public LayerMask playerLayer;

    public Transform eyesPoint;
    public float visionRadius = 0.3f;

    [Header("Alert Settings")]
    public AlertController alertController;

    [Header("Vision Settings")]
    public float detectRange = 10f;
    public float alertIncreaseAmount = 30f;
    public float alertInterval = 0.5f;
    private float currentAlertTimer = 0f;

    [SerializeField] private bool isInPrivateArea = false;

    // ===== 世界空间 UI（场景里的，不是 Prefab）=====
    [Header("UI Feedback")]
    public GameObject alertUI;          // 场景中的 World Space UI（默认隐藏）
    public float alertUIDuration = 2f;  // 显示多久

    // 防止一直连续报警
    private bool hasAlertedPolice = false;

    private void Start()
    {
        if (alertController == null)
            alertController = GetComponent<AlertController>();

        // 确保一开始是隐藏的
        if (alertUI != null)
            alertUI.SetActive(false);
    }

    void Update()
    {
        if (eyesPoint == null) return;
        CheckVision();
    }

    // ===== Private Area 检测 =====
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PrivateArea"))
        {
            isInPrivateArea = true;
            currentAlertTimer = 0f;
        }
    }

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

        LayerMask combinedMask = stealableLayer | playerLayer;

        if (Physics.SphereCast(startPos, visionRadius, direction, out hit, detectRange, combinedMask))
        {
            GameObject hitObj = hit.collider.gameObject;

            // =================================================
            // A. 私人区域看到玩家
            // =================================================
            if (isInPrivateArea && hitObj.CompareTag("Player"))
            {
                Debug.Log("See Player");

                if (alertController != null && !alertController.isAlerted)
                {
                    currentAlertTimer += Time.deltaTime;

                    if (currentAlertTimer >= alertInterval)
                    {
                        alertController.IncreaseAlert(alertIncreaseAmount);
                        Debug.Log("私人区域发现玩家！警戒值增加！");
                        currentAlertTimer = 0f;
                    }
                }

                if (alertUI != null && !hasAlertedPolice)
                {
                    alertUI.SetActive(true);
                    CancelInvoke(nameof(HideAlertUI));
                    Invoke(nameof(HideAlertUI), alertUIDuration);
                }

                return;
            }

            // =================================================
            // B. 抓到偷窃
            // =================================================
            StealableObject target = hitObj.GetComponentInParent<StealableObject>();

            if (target != null && target.IsBeingStolen)
            {
                Debug.LogError("NPC: 抓到你了！执行强制打断！");
                target.HandleFailure(true);

                // 1. UI 反馈（世界 UI）
                if (alertUI != null && !hasAlertedPolice)
                {
                    alertUI.SetActive(true);
                    CancelInvoke(nameof(HideAlertUI));
                    Invoke(nameof(HideAlertUI), alertUIDuration);
                }

                // 2. 呼叫警察
                if (!hasAlertedPolice)
                {
                    Debug.Log("NPC: 来人啊！抓小偷！");

                    if (PoliceManager.Instance != null)
                        PoliceManager.Instance.DispatchNearestPolice(transform.position);

                    hasAlertedPolice = true;

                    if (alertController != null)
                        alertController.IncreaseAlert(alertController.maxAlertValue);

                    Invoke(nameof(ResetAlert), 5f);
                }
            }
        }
        else
        {
            currentAlertTimer = 0f;
        }
    }

    void HideAlertUI()
    {
        if (alertUI != null)
            alertUI.SetActive(false);
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
            Gizmos.DrawWireSphere(eyesPoint.position, visionRadius);
            Gizmos.DrawRay(eyesPoint.position, eyesPoint.forward * detectRange);
        }
    }
}