using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
// 如果你是 XRI 2.x 或 3.x，请保留/删除下行
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class StealableObject : XRGrabInteractable
{
    [Header("Steal Settings")]
    [Range(1, 10)]
    public int difficultyLevel = 3;

    [Header("Objects Disabled During Gesture")]
    [Tooltip("⚠️注意：只拖入【传送点、射线、菜单键】等环境物体。\n❌不要拖入手势UI或弹窗，否则成功后会被强制显示出来！")]
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

        if (!_isStealing)
        {
            StartCoroutine(StartStealingProcess());
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // 玩家中途自己松手 → 判为失败
        if (_isStealing)
        {
            HandleFailure(false);
        }
    }

    private IEnumerator StartStealingProcess()
    {
        _isStealing = true;
        Debug.Log($"[{gameObject.name}] 开始偷窃验证...");

        // 1. 关闭干扰物体（传送、射线等）
        SetObjectsActive(false);

        // 2. 启动手势游戏
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

    // ================== 核心逻辑修正区 ==================

    public void HandleSuccess()
    {
        if (!_isStealing) return;
        _isStealing = false;

        Debug.Log($"[{gameObject.name}] 偷窃成功！");

        // 1. 【新增】先强制松手。物体要消失了，手必须先放开，否则会卡住手的状态
        if (isSelected && interactionManager != null)
        {
            interactionManager.SelectExit(firstInteractorSelecting, this);
        }

        // 2. 通知 Manager 任务完成（让它去关闭手势 UI）
        // 这一步必须在 SetObjectsActive 之前，防止 UI 冲突
        if (GestureGameManager.Instance != null)
        {
            // 传 true 代表成功，Manager 内部应该处理关闭 UI 的逻辑
            GestureGameManager.Instance.CompleteChallenge(true);
        }

        // 3. 恢复环境物体（射线、传送等）
        SetObjectsActive(true);

        // 4. 物体消失
        gameObject.SetActive(false);
    }

    public void HandleFailure(bool forceDrop = true)
    {
        // 防止递归死锁：如果已经处理过失败，直接返回
        if (!_isStealing) return;

        _isStealing = false;
        Debug.Log($"[{gameObject.name}] 偷窃失败/被抓！");

        // 1. 【关键修改】优先执行物理松手！
        // 不管后面逻辑有没有报错，先让玩家把东西掉下来
        if (forceDrop && isSelected && interactionManager != null)
        {
            interactionManager.SelectExit(firstInteractorSelecting, this);
        }

        // 2. 通知 Manager 关闭手势 UI
        if (GestureGameManager.Instance != null)
        {
            // 告诉 Manager 失败了，请关闭 UI
            GestureGameManager.Instance.CompleteChallenge(false);
        }

        // 3. 停止所有协程（防止还有计时器在跑）
        StopAllCoroutines();

        // 4. 恢复环境物体
        SetObjectsActive(true);

        // 5. 物体归位
        StartCoroutine(ReturnToOriginalPositionRoutine());
    }

    private IEnumerator ReturnToOriginalPositionRoutine()
    {
        // 等待物理帧结束，确保松手动作完成
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

    // ================== 工具函数 ==================

    private void SetObjectsActive(bool active)
    {
        if (objectsToDisable == null) return;

        foreach (var obj in objectsToDisable)
        {
            if (obj != null)
            {
                // 这里加个保险：如果这个物体本来就是 UI 的一部分，
                // 在 active=true (恢复) 时，可能会错误地开启 UI。
                // 建议：检查 objectsToDisable 列表，不要把 UI 面板拖进去！
                obj.SetActive(active);
            }
        }
    }
}