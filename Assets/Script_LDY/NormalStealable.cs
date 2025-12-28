using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InstantStealObject : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    // ================= 保持你的原有设置 =================
    [Header("Inventory Settings")]
    [Tooltip("请把这个物体对应的 ItemData (比如 Gold, Apple) 拖到这里")]
    public ItemData itemData;
    // ==========================================================

    private Vector3 _originalPosition;
    private Quaternion _originalRotation;
    private bool _isStealing = false;

    protected override void Awake()
    {
        base.Awake();
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        // 一旦被抓取，直接执行成功逻辑
        if (!_isStealing)
        {
            HandleSuccess();
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        _isStealing = false;
    }

    public void HandleSuccess()
    {
        // --- 修改点：不再盲目添加，而是获取添加结果 ---
        bool addSuccess = false;

        // 1. 尝试加入背包
        if (InventoryManager.Instance != null && itemData != null)
        {
            // AddItem 现在返回 bool：true代表成功加入，false代表满了
            addSuccess = InventoryManager.Instance.AddItem(itemData);
        }
        else
        {
            Debug.LogWarning("注意：缺少 Manager 或 ItemData");
        }

        // 2. 根据结果决定物体命运
        if (addSuccess)
        {
            // A. 成功：标记偷盗状态，物体消失
            _isStealing = true;
            Debug.Log("偷盗成功！物体消失");

            // 强制松手
            if (isSelected)
            {
                interactionManager.SelectExit(firstInteractorSelecting, this);
            }

            // 物体消失
            gameObject.SetActive(false);
        }
        else
        {
            // B. 失败（背包满了）：重置状态，物体保留
            _isStealing = false;
            Debug.Log("偷盗失败：背包已满或受限，物体掉落");

            // 强制松手（让物体掉回桌子上，而不是粘在手上）
            if (isSelected)
            {
                interactionManager.SelectExit(firstInteractorSelecting, this);
            }

            // 注意：这里没有 SetActive(false)，所以物体还在
        }
    }

    public void HandleFailure()
    {
        if (!_isStealing && !gameObject.activeSelf) return;

        _isStealing = false;
        Debug.Log("偷盗失败！回到原位");

        if (isSelected)
        {
            interactionManager.SelectExit(firstInteractorSelecting, this);
        }

        transform.position = _originalPosition;
        transform.rotation = _originalRotation;

        if (GetComponent<Rigidbody>())
        {
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}