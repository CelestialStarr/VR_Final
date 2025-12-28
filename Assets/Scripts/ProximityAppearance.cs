using UnityEngine;

public class ProximityAppearance : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("玩家进入这个距离范围内，物体出现")]
    public float showDistance = 1.5f;

    [Tooltip("玩家离开这个距离范围后，物体消失 (建议比Show Distance大一点，防止闪烁)")]
    public float hideDistance = 2.0f;

    [Tooltip("缩放动画的速度")]
    public float smoothSpeed = 10f;

    [Header("References")]
    [Tooltip("如果不填，默认自动寻找主摄像机")]
    public Transform playerCamera;

    [Tooltip("如果不填，默认控制挂载这个脚本的物体")]
    public Transform targetTransform;

    // 记录原始大小，防止物体原本不是(1,1,1)
    private Vector3 initialScale;
    private bool isShowing = false;
    private Collider[] childColliders;
    private CanvasGroup canvasGroup;

    void Start()
    {
        // 1. 自动寻找组件
        if (playerCamera == null)
        {
            if (Camera.main != null)
                playerCamera = Camera.main.transform;
            else
                Debug.LogError("找不到 MainCamera，请手动赋值 Player Camera！");
        }

        if (targetTransform == null)
            targetTransform = transform;

        // 2. 记录原始大小
        initialScale = targetTransform.localScale;

        // 3. 获取所有碰撞体/UI组件 (为了在隐藏时关闭交互)
        childColliders = targetTransform.GetComponentsInChildren<Collider>();
        canvasGroup = targetTransform.GetComponent<CanvasGroup>();

        // 初始设为隐藏状态 (大小为0)
        targetTransform.localScale = Vector3.zero;
        ToggleInteractability(false);
    }

    void Update()
    {
        if (playerCamera == null) return;

        // 计算距离
        float distance = Vector3.Distance(playerCamera.position, targetTransform.position);

        //Debug.Log($"当前距离: {distance}, 目标距离: {showDistance}, 当前Scale: {targetTransform.localScale}");

        // 状态机逻辑 (滞后阈值，防止在边缘反复闪烁)
        if (!isShowing && distance < showDistance)
        {
            isShowing = true; // 进入显示范围
            ToggleInteractability(true);
        }
        else if (isShowing && distance > hideDistance)
        {
            isShowing = false; // 离开隐藏范围
            ToggleInteractability(false);
        }

        // 执行平滑缩放动画
        Vector3 targetScaleVec = isShowing ? initialScale : Vector3.zero;
        targetTransform.localScale = Vector3.Lerp(targetTransform.localScale, targetScaleVec, Time.deltaTime * smoothSpeed);

        // 可选：如果完全看不见了，强制设为0避免微小计算
        if (!isShowing && targetTransform.localScale.x < 0.01f)
        {
            targetTransform.localScale = Vector3.zero;
        }
    }

    // 开启/关闭交互功能（防止看不见的时候还能按）
    void ToggleInteractability(bool state)
    {
        // 如果是物理碰撞体
        foreach (var col in childColliders)
        {
            col.enabled = state;
        }

        // 如果是UI Canvas
        if (canvasGroup != null)
        {
            canvasGroup.interactable = state;
            canvasGroup.blocksRaycasts = state;
        }
    }

    // 在编辑器里画个圈，方便调试距离
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, showDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hideDistance);
    }
}

