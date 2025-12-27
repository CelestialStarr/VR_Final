using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class InstantStealObject : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    // ================= 修改点 1：添加物品数据槽位 =================
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
        _isStealing = true;
        Debug.Log("偷盗成功！物体消失");

        // ================= 修改点 2：调用背包管理器 =================
        // 确保背包管理器存在，并且你记得把 ItemData 拖进Inspector了
        if (InventoryManager.Instance != null && itemData != null)
        {
            // 调用我们在 InventoryManager 里写的 AddItem 方法
            InventoryManager.Instance.AddItem(itemData);
        }
        else
        {
            // 如果你发现偷了东西背包没增加，大概率是忘了在 Inspector 里拖 ItemData
            Debug.LogWarning("注意：偷窃成功，但未加入背包。请检查 InventoryManager 是否在场景中，或者 ItemData 是否已赋值。");
        }
        // ==========================================================

        // 强制松手
        if (isSelected)
        {
            interactionManager.SelectExit(firstInteractorSelecting, this);
        }

        // 物体消失
        gameObject.SetActive(false);
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