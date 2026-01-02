using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
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
    private bool _isStealing = false;

    public bool IsBeingStolen => _isStealing;

    protected override void Awake()
    {
        base.Awake();
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (_isStealing) return;

        // 1. 背包容量与限制检查
        if (InventoryManager.Instance != null && itemData != null)
        {
            int currentTotal = 0;
            foreach (var slot in InventoryManager.Instance.backpackContent)
            {
                currentTotal += slot.stackSize;
            }

            bool isBagFull = currentTotal >= InventoryManager.Instance.maxTotalCapacity;

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

            if (isBagFull)
            {
                InventoryManager.Instance.TriggerBagFullEvent();
                interactionManager.SelectExit(args.interactorObject, this);
                return;
            }

            if (isLimitReached)
            {
                InventoryManager.Instance.TriggerItemLimitEvent($"最多只能获取{InventoryManager.Instance.maxLimitedItemCount}个包裹");
                interactionManager.SelectExit(args.interactorObject, this);
                return;
            }
        }

        StartCoroutine(StartStealingProcess());
    }

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

        Debug.Log($"[{gameObject.name}] 偷窃成功，物体消失并尝试入包。");

        if (InventoryManager.Instance != null && itemData != null)
        {
            InventoryManager.Instance.AddItem(itemData);
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
        // 无论入包是否成功，物体直接消失
        gameObject.SetActive(false);
    }

    public void HandleFailure(bool forceDrop = true)
    {
        if (!_isStealing) return;
        _isStealing = false;

        Debug.Log($"[{gameObject.name}] 偷窃失败，物体直接消失。");

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

        // 修改：不再执行返回原位的逻辑，直接隐藏
        gameObject.SetActive(false);
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