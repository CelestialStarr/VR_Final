using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
// 如果你是 XR Interaction Toolkit 3.x 请保留下行，2.x 请删除
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class StealableObject : XRGrabInteractable
{
    [Header("Inventory Settings")]
    [Tooltip("必须拖入 ItemData，否则无法判断")]
    public ItemData itemData;

    [Header("Steal Settings")]
    [Range(1, 10)]
    public int difficultyLevel = 3;

    [Header("Objects Disabled During Gesture")]
    public List<GameObject> objectsToDisable;

    // --- 状态变量 ---
    private Vector3 _originalPosition;
    private Quaternion _originalRotation;

    // 内部私有变量
    private bool _isStealing = false;

    // ★★★ 修复点：添加这个公共属性，让外部脚本能读取状态 ★★★
    public bool IsBeingStolen => _isStealing;

    protected override void Awake()
    {
        base.Awake();
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    // =========================================================
    // ★ 核心：抓取瞬间的预检查
    // =========================================================
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (_isStealing) return;

        // 1. 开始检查背包
        if (InventoryManager.Instance != null && itemData != null)
        {
            // --- A. 计算当前总量 ---
            int currentTotal = 0;
            foreach (var slot in InventoryManager.Instance.backpackContent)
            {
                currentTotal += slot.stackSize;
            }

            bool isBagFull = currentTotal >= InventoryManager.Instance.maxTotalCapacity;

            // --- B. 计算特殊物品限制 ---
            bool isLimitReached = false;
            if (InventoryManager.Instance.unsellableItem != null &&
                itemData == InventoryManager.Instance.unsellableItem)
            {
                var slot = InventoryManager.Instance.backpackContent.Find(s => s.itemData == itemData);
                int currentCount = (slot != null) ? slot.stackSize : 0;

                if (currentCount >= InventoryManager.Instance.maxLimitedItemCount)
                {
                    isLimitReached = true;
                }
            }

            // --- C. 结果判定 ---
            if (isBagFull)
            {
                Debug.Log("【预检查】背包已满。");

                // 1. 触发背包管理器的UI事件
                InventoryManager.Instance.TriggerBagFullEvent();

                // 2. 强制松手
                interactionManager.SelectExit(args.interactorObject, this);

                // 3. 终止流程
                return;
            }

            if (isLimitReached)
            {
                Debug.Log("【预检查】限购已满。");

                // 1. 触发限购UI
                InventoryManager.Instance.TriggerItemLimitEvent($"最多只能获取{InventoryManager.Instance.maxLimitedItemCount}个包裹");

                // 2. 强制松手
                interactionManager.SelectExit(args.interactorObject, this);

                // 3. 终止流程
                return;
            }
        }

        // 2. 只有上面检查都通过了，才开始手势游戏
        StartCoroutine(StartStealingProcess());
    }

    // =========================================================
    // 下面的代码保持原样
    // =========================================================

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        if (_isStealing)
        {
            HandleFailure(false);
        }
    }

    private IEnumerator StartStealingProcess()
    {
        _isStealing = true;
        Debug.Log($"[{gameObject.name}] 空间充足，开始手势挑战...");

        SetObjectsActive(false);

        if (GestureGameManager.Instance != null)
        {
            GestureGameManager.Instance.StartValidation(this, difficultyLevel);
        }
        else
        {
            Debug.LogError("场景缺少 GestureGameManager！");
            HandleFailure();
        }

        yield return null;
    }

    public void HandleSuccess()
    {
        if (!_isStealing) return;
        _isStealing = false;

        Debug.Log($"[{gameObject.name}] 手势挑战成功！入包...");

        bool finalSuccess = false;
        if (InventoryManager.Instance != null && itemData != null)
        {
            finalSuccess = InventoryManager.Instance.AddItem(itemData);
        }

        if (isSelected && interactionManager != null)
        {
            interactionManager.SelectExit(firstInteractorSelecting, this);
        }

        if (GestureGameManager.Instance != null)
        {
            GestureGameManager.Instance.CompleteChallenge(true);
        }

        SetObjectsActive(true);

        if (finalSuccess)
        {
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("入包失败，物品掉落");
        }
    }

    public void HandleFailure(bool forceDrop = true)
    {
        if (!_isStealing) return;
        _isStealing = false;
        Debug.Log($"[{gameObject.name}] 偷窃失败/被抓！");

        if (forceDrop && isSelected && interactionManager != null)
        {
            interactionManager.SelectExit(firstInteractorSelecting, this);
        }

        if (GestureGameManager.Instance != null)
        {
            GestureGameManager.Instance.CompleteChallenge(false);
        }

        StopAllCoroutines();
        SetObjectsActive(true);
        StartCoroutine(ReturnToOriginalPositionRoutine());
    }

    private IEnumerator ReturnToOriginalPositionRoutine()
    {
        yield return new WaitForFixedUpdate();
        transform.position = _originalPosition;
        transform.rotation = _originalRotation;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void SetObjectsActive(bool active)
    {
        if (objectsToDisable == null) return;
        foreach (var obj in objectsToDisable)
        {
            if (obj != null) obj.SetActive(active);
        }
    }
}